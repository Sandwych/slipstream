using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Sandwych.Threading
{
 
    public class AsyncEnumerator
    {

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected IAsyncResult m_asyncResult;
        // Refers to the iterator member's code
        private IEnumerator<Int32> m_enumerator;
        // Collection of completed async operations (the inbox)
        private List<IAsyncResult> m_inbox =new List<IAsyncResult>();
         // Structure with Wait & Inbox counters
        private WaitAndInboxCounts m_waitAndInboxCounts;
        private void ResumeIterator()
        {
            Boolean continueIterating;

            // While there are more operations to perform...
            while (continueIterating = m_enumerator.MoveNext())
            {

                // Get the value returned from the enumerator
                UInt16 numberOpsToWaitFor = checked((UInt16)m_enumerator.Current);

                // If inbox has fewer items than requested, keep iterator suspended
                if (!m_waitAndInboxCounts.AtomicSetWait(numberOpsToWaitFor)) break;

                // Inbox has enough items, loop to resume the iterator
            }

            // The iterator is suspended, just return
            if (continueIterating) return;

            // The iterator has exited, execute the iterator's finally code
            m_enumerator.Dispose();
        }
        private void EnqueueAsyncResult(IAsyncResult result)
        {
            // Add this item to the inbox
            lock (m_inbox) { m_inbox.Add(result); }

            // Add 1 to inbox count. If inbox has enough items 
            // in it; this thread calls ResumeIterator
            if (m_waitAndInboxCounts.AtomicIncrementInbox())
                ResumeIterator();
        }
        private struct WaitAndInboxCounts
        {
            private const UInt16 c_MaxWait = 0xFFFF;
            // Wait=High 16 bits, Inbox=low-16 bits
            private Int32 m_waitAndInboxCounts;

            private UInt16 Wait
            {
                get { return (UInt16)(m_waitAndInboxCounts >> 16); }
                set { m_waitAndInboxCounts = (Int32)((value << 16) | Inbox); }
            }
            private UInt16 Inbox
            {
                get { return (UInt16)m_waitAndInboxCounts; }
                set
                {
                    m_waitAndInboxCounts =
                        (Int32)((m_waitAndInboxCounts &
                    0xFFFF0000) | value);
                }
            }

            private WaitAndInboxCounts(Int32 waic) { m_waitAndInboxCounts = waic; }
            private Int32 ToInt32() { return m_waitAndInboxCounts; }

            internal void Initialize() { Wait = c_MaxWait; }

            internal Boolean AtomicSetWait(UInt16 numberOpsToWaitFor)
            {
                return InterlockedEx.Morph<Boolean, UInt16>(
                   ref m_waitAndInboxCounts,
                   numberOpsToWaitFor, SetWait);
            }

            private static Int32 SetWait(Int32 i, UInt16 numberOpsToWaitFor,
               out Boolean shouldMoveNext)
            {
                WaitAndInboxCounts waic = new WaitAndInboxCounts(i);
                // Set the number of items to wait for
                waic.Wait = numberOpsToWaitFor;
                shouldMoveNext = (waic.Inbox >= waic.Wait);

                // Does the inbox contain enough items to MoveNext?
                if (shouldMoveNext)
                {
                    // Subtract the number of items from the inbox 
                    waic.Inbox -= waic.Wait;
                    // The next wait is indefinite 
                    waic.Wait = c_MaxWait;
                }
                return waic.ToInt32();
            }

            internal Boolean AtomicIncrementInbox()
            {
                return InterlockedEx.Morph<Boolean, Object>(
                   ref m_waitAndInboxCounts,
                   null, IncrementInbox);
            }

            private static Int32 IncrementInbox(Int32 i, Object argument,
               out Boolean shouldMoveNext)
            {
                WaitAndInboxCounts waic = new WaitAndInboxCounts(i);
                // Add 1 to the inbox count
                waic.Inbox++;
                shouldMoveNext = (waic.Inbox == waic.Wait);

                // Does the inbox contain enough items to MoveNext?
                if (shouldMoveNext)
                {
                    // Subtract the number of items from the inbox 
                    waic.Inbox -= waic.Wait;
                    // The next wait is indefinite 
                    waic.Wait = c_MaxWait;
                }
                return waic.ToInt32();
            }
        }
        public AsyncEnumerator()
        {
 
            this.m_waitAndInboxCounts = new WaitAndInboxCounts();
            this.m_waitAndInboxCounts.Initialize();
         }
        public IAsyncResult  BeginExecute(IEnumerator<int> enumerator, AsyncCallback callback)
        {
            return this.BeginExecute(enumerator, callback, null);
        }
        public IAsyncResult  BeginExecute(IEnumerator<int> enumerator, AsyncCallback callback, object state)
        {
            this.m_enumerator = enumerator;
            this.ResumeIterator();
            return this.m_asyncResult;
        }
        public IAsyncResult  DequeueAsyncResult()
        {
            IAsyncResult asyncResultWrapper;
            lock (this.m_inbox)
            {
                asyncResultWrapper = this.m_inbox[0];
                this.m_inbox.RemoveAt(0);
            }
            return asyncResultWrapper;
        }
        public AsyncCallback End()
        {
            return this.EnqueueAsyncResult;
        }
        public void EndExecute(IAsyncResult result)
        {
           // try
           // {
           //     this.m_asyncResult.EndInvoke();
           // }
           // finally
           // {
                this.EndExecuteCleanup();
           // }
        }
        internal void EndExecuteCleanup()
        {
            
            this.m_enumerator = null;
 
        }
        public void Execute(IEnumerator<Int32> enumerator)
        {
            //  this.EndExecute(this.BeginExecute(enumerator, null, null));
             this.BeginExecute(enumerator, null, null);
        }
        public void Cancel()
        {
            this.ResumeIterator();
        }

    }
 

}
