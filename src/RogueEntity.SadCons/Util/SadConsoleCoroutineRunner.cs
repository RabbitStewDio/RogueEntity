using Microsoft.Xna.Framework;
using RogueEntity.Api.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using Game = SadConsole.Game;

namespace RogueEntity.SadCons.Util
{
    public class SadConsoleCoroutineRunner
    {
        static readonly Lazy<SadConsoleCoroutineRunner> instance = new Lazy<SadConsoleCoroutineRunner>();
        public static SadConsoleCoroutineRunner Instance => instance.Value;

        readonly BufferList<(int, IEnumerator)> buffered;
        readonly List<(CoroutineHandle handle, IEnumerator coroutine)> activeCoroutines;

        SadConsoleCoroutineRunner()
        {
            buffered = new BufferList<(int, IEnumerator)>();
            activeCoroutines = new List<(CoroutineHandle, IEnumerator)>();
            Game.OnDestroy += OnExiting;
            Game.OnUpdate += OnUpdate;
        }

        public CoroutineHandle Start(IEnumerator e)
        {
            var handle = CoroutineHandle.Create();
            activeCoroutines.Add((handle, e));
            return handle;
        }
        
        public void Cancel(CoroutineHandle handle)
        {
            activeCoroutines.RemoveAll(e => e.handle == handle);
        }

        void OnUpdate(GameTime obj)
        {
            buffered.Clear();
            for (var i = activeCoroutines.Count - 1; i >= 0; i--)
            {
                buffered.Add((i, activeCoroutines[i].coroutine));
            }

            try
            {
                for (var i = 0; i < buffered.Count; i++)
                {
                    var c = buffered[i];
                    if (!c.Item2.MoveNext())
                    {
                        activeCoroutines.RemoveAt(c.Item1);
                    }
                }
            }
            finally
            {
                buffered.Clear();
            }
        }

        void OnExiting()
        {
            activeCoroutines.Clear();
        }
    }
}
