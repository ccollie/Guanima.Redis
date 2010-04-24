using System;

namespace Guanima.Redis.Utils
{
    public class DisposableAction<T> : Disposable
    {
        private readonly Action<T> _action;
        private readonly T _val;

        public DisposableAction(Action<T> action, T val)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            _action = action;
            _val = val;
        }

        protected override void  Release()
        {
            _action(_val);
        }
    }
}
