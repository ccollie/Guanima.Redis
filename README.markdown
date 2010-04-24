# Guanima.Redis #

## About ##

Guanima.Redis is an experimental client library for the Redis key-value store.

It is based on ideas and code from other open-source clients, and adds functionality specific 
to the author's use-case.

The code is currently in alpha, and not suitable for production use. Unit tests are
included, so you can get a sense of the usage patterns.


## Main features ##

- Connection pooling
- Pluggable key-transformations, including built-in support for namespaces
- Key tag support to influence the server on which a key is stored
- Transparent key distribution through client-side sharding (support for consistent hashing and custom distribution algorithms)
- Pipelining support
- Redis transaction support using IDisposable
- Pluggle serialization for POCO classes
- Built-in support for cluster-wide locking using SETNX


### Connecting to a local instance of Redis ###

You don't have to specify a tcp host and port when connecting to Redis instances 
running on the localhost on the default port:

    var client = new RedisClient();
    client["library"] = "hello world";
    var value = client["library"];

### External Configuration ##

	
	<?xml version="1.0" encoding="utf-8" ?>
	<configuration>
		<configSections>
			<sectionGroup name="Guanima">
				<section name="Redis" type="Guanima.Redis.Configuration.RedisClientSection, Guanima.Redis" />
			</sectionGroup>

			<sectionGroup name="RedisConfig">
				<section name="Local" type="Guanima.Redis.Configuration.RedisClientSection, Guanima.Redis" />
				<section name="Staging" type="Guanima.Redis.Configuration.RedisClientSection, Guanima.Redis" />
				<section name="Production" type="Guanima.Redis.Configuration.RedisClientSection, Guanima.Redis" />
			</sectionGroup>
		</configSections>
		<Guanima> <!-- default config section used if none specified -->
			<Redis>
				<servers>
					<add address="127.0.0.1" port="6379" />
					<add address="127.0.0.1" port="6380" />
					<add address="127.0.0.1" port="6381" />
				</servers>
				<socketPool minPoolSize="10" maxPoolSize="100" connectionTimeout="00:00:10" />
			</Redis>
		</Guanima>

		<RedisConfig> 
			<Local>
				<servers>
					<add address="127.0.0.1" port="6379" />
				</servers>
				<socketPool minPoolSize="10" maxPoolSize="100" connectionTimeout="00:00:40" />
			</Local>
			<Staging>
				<servers>
					<add address="cache0.devserver.com" port="6379" alias="master"/>
					<add address="cache1.devserver.com" port="6379" alias="storm" />
					<add address="cache2.devserver.com" alias="banshee" />
				</servers>
				<socketPool minPoolSize="10" maxPoolSize="100" connectionTimeout="00:00:10" />
			</Staging>
			<Production>
				<servers>
					<add address="cache0.realsite.com" port="6379" alias="master" />
					<add address="cache1.realsite.com" port="6379" alias="storm" />
					<add address="cache2.realsite.com" alias="banshee" />
				</servers>
				<socketPool minPoolSize="10" maxPoolSize="100" connectionTimeout="00:00:10" />
			</Production>
		</RedisConfig>
	</configuration>

	var client = new RedisClient();  // uses the section Guanima/Redis 
	var client1 = new RedisClient("Guanima/Redis");  // same as above
	var local = new RedisClient("RedisConfig/Local");  // local server config
	var staging = new RedisClient("RedisConfig/Staging");  // Staging server config
	var production = new RedisClient("RedisConfig/Production");  // Production server config
	
	
### Programmatic configuration ##
	
	public RedisClient CreateSingleNodeClient()
	{
		var mcc = new RedisClientConfiguration();
		var server = new EndPointConfiguration(new IPEndPoint(IPAddress.Loopback, 6379));
		mcc.Servers.Add(server);

		mcc.NodeLocator = typeof(SingleNodeLocator);
		mcc.KeyTransformer = typeof(DefaultKeyTransformer);
		mcc.Transcoder = typeof(DefaultTranscoder);

		mcc.SocketPool.MinPoolSize = 10;
		mcc.SocketPool.MaxPoolSize = 100;
		mcc.SocketPool.ConnectionTimeout = new TimeSpan(0, 0, 10);
		mcc.SocketPool.DeadTimeout = new TimeSpan(0, 0, 30);
		return new RedisClient(mcc);
	}

### Pipelining multiple commands to a remote instance of Redis ##

Pipelining helps with performances when there is the need to issue many commands 
to a server in one go:

    var client = RedisClient();
	using(client.BeginPipeline()) 
	{
        client.IncrBy("counter", 10);
        client.IncrBy("counter", 30);
        client.Del("other-value");
    });


### Pipelining multiple commands to multiple instances of Redis (sharding) ##

Guanima.Redis supports data sharding using consistent-hashing on keys on the client side. 
Furthermore, a pipeline can be initialized on a cluster of redis instances in the 
same exact way they are created on single connection. Sharding is still transparent 
to the user:

    var redis = new RedisClient("RedisConfig/Staging");

	using(var r = client.BeginPipeline())
	{
        for (int i = 0; i < 1000; i++) 
		{
            client.Set("key:" + i, i);
            client.Set( String.Format("user:{0}:points", i), i);
        }
    });

### Transactions ##

	[Test]
	public void Test_Transaction_Basics() 
	{
		r.Del("mylist");
		r.RPush("mylist", "a");
		r.RPush("mylist", "b");
		r.RPush("mylist", "c");
		RedisCommand lRangeCommand;
		using (var trans = r.BeginTransaction())
		{
			r.LRange("mylist", 0, -1);
			r.Set("scalar", 1);
			r.Incr("scalar");
			var cmds = trans.Commands.ToList();
			lRangeCommand = cmds[0];
		}
		var expected = new[] {"a", "b", "c"};
		AssertListsAreEqual(expected, lRangeCommand.Result);            
	} 
	
### Specify a server for a command ###

	var client = new RedisClient("RedisConfig/Staging");
	
	long count;
	using (client.On("tigger")) 
	{
		// will increment count on server with alias "tigger", regardless
		// of number of servers in the cluster or distribution algorithm
		count = client.Incr("global_counter");
	}

### Locking ##

	[Test]
	public void A_Lock_Should_Delete_Its_Redis_Key_Before_Leaving_Scope()
	{
		using (r.Lock(LockName))
		{
			Assert.That(r.Exists(LockName));
		}
		Assert.That(!r.Exists(LockName));
		
		using (r.Lock(LockName, TimeSpan.Zero, TimeSpan.FromDays(1)))
		{
			Assert.That(r.Exists(LockName));
		}
		Assert.That(!r.Exists(LockName));
	}


	[Test]
	public void Acquiring_A_Lock_Should_Prevent_Another_Client_From_Acquiring_It()
	{
		using (r.Lock(LockName))
		{
			Assert.That(r.Exists(LockName));
			using(var newClient = GetClient())
			{
		 		Assert.Throws(typeof (RedisLockTimeoutException),
							  () =>
								  {
									  using (newClient.Lock(LockName, TimeSpan.FromSeconds(2)))
									  {
										  Assert.Fail("Lock was acquired twice.");
									  }
								  });
			}
		}            
	}

### Key Tags ##
    
	From the Redis Wiki:
    
	A key tag is a special pattern inside a key that, if present, is the only 
    part of the key hashed in order to select the server for this key. 
    For example in order to hash the key "foo" I simply perform the hash of the 
    whole string, but if this key has a pattern in the form of the characters {...} I only hash this 
    substring. So for example for the key "foo{bared}" the key hashing code will simply perform the hash of "bared". 
    This way using key tags you can ensure that related keys will be stored on the same Redis instance just using the same 
    key tag for all this keys. 
    
	
	
## Development ##

Guanima.Redis is alpha level and is still in heavy development.
Suggestions, feedback and pull-requests welcome.

## Dependencies ##
- .NET 3.5
- Log4Net
- NUnit (needed to run the test suite)

## Links ##

### Project ###
- [Source code](http://github.com/ccollie/Guanima.Redis/)
- [Wiki](http://wiki.github.com/ccollie/Guanima.Redis/)
- [Issue tracker](http://github.com/ccollie/Guanima.Redis/issues)

### Related ###
- [Redis](http://code.google.com/p/redis/)

## Author ##

[clayton collie] (http://github.com/ccollie)

## Credits/Thanks ##

[Attila Kiskó]	(Enyim.Memcached) 
[Daniele Alessandri] (Predis) (mailto:suppakilla@gmail.com)
[Ryan Petrich] (http://github.com/rpetrich)
[Demis Bellot] (ServiceStack)

## License ##

The code for Guanima.Redis is distributed under the terms of the Apache license (see LICENSE).
