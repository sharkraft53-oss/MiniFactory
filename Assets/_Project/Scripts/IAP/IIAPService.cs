using System;

namespace MiniFactory.IAP
{
    public interface IIAPService
    {
        bool IsInitialized { get; }

        event Action<string> PurchaseSucceeded;
        event Action<string, string> PurchaseFailed;
        event Action<string> InitializationFailed;

        void Initialize();
        void BuyProduct(string productId);
    }
}