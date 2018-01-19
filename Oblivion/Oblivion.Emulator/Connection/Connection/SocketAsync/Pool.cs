using System;
using System.Collections.Generic;

namespace Oblivion.Connection.Connection.SocketAsync
{
    internal class Pool<T>
    {
        public int Limit
        {
            get;
            private set;
        }

        protected Stack<T> BasicStack
        {
            get;
            private set;
        }

        public Pool(int Limit)
        {
            this.Limit = Limit;
            this.BasicStack = new Stack<T>(Limit);
        }

        public T PushAndHandle()
        {
            T Obj = (T)typeof(T).GetConstructor(new Type[] { }).Invoke(new object[] { });
            BasicStack.Push(Obj);
            return Obj;
        }

        public void Push(T Obj)
        {
            this.BasicStack.Push(Obj);
        }

        public ICollection<T> PushAndHandleAll()
        {
            ICollection<T> Output = new List<T>();

            for (int i = 0; i < Limit; i++)
            {
                Output.Add(PushAndHandle());
            }

            return Output;
        }

        public bool TryPop(out T Output)
        {
            Output = BasicStack.Pop();
            return Output != null;
        }
    }
}