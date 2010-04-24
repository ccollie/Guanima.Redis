using System;

namespace Guanima.Redis.Utils
{
    public class DisposableAction : Disposable
    {
        private readonly Action _action;

        public DisposableAction(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            _action = action;
        }

        protected override void Release()
        {
            _action();
        }

    }
}
