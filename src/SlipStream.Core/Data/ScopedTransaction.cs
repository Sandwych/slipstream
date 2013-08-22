using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace SlipStream.Data
{

    //此类的原作者为 Orchard 项目
    public interface IScopedTransaction
    {
        void Demand();
        void Cancel();
    }

    //此类的原作者为 Orchard 项目
    public class ScopedTransaction : IScopedTransaction, IDisposable
    {
        private TransactionScope _scope;
        private bool _cancelled;
        private static readonly TransactionOptions Options = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted
        };

        public ScopedTransaction()
        {
        }

        void IScopedTransaction.Demand()
        {
            if (this._scope == null)
            {
                this._scope = new TransactionScope(
                    TransactionScopeOption.Required, Options);
            }
        }

        void IScopedTransaction.Cancel()
        {
            this._cancelled = true;
        }

        void IDisposable.Dispose()
        {
            if (this._scope != null)
            {
                if (!this._cancelled)
                {
                    this._scope.Complete();
                }

                this._scope.Dispose();
            }
        }

    }
}
