using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace MiniFactory.IAP
{
    public class UnityIAPService : IIAPService
    {
        private StoreController _storeController;
        private string _lastRequestedProductId;

        public bool IsInitialized { get; private set; }

        public event Action<string> PurchaseSucceeded;
        public event Action<string, string> PurchaseFailed;
        public event Action<string> InitializationFailed;

        public async void Initialize()
        {
            if (IsInitialized)
                return;

            try
            {
                _storeController =
                    UnityIAPServices.StoreController();

                SubscribeEvents();

                Debug.Log(
                    "[IAP] Connecting to store..."
                );

                await _storeController.Connect();
            }
            catch (Exception exception)
            {
                HandleInitializationFailure(
                    exception.Message
                );
            }
        }

        public void BuyProduct(string productId)
        {
            if (!IsInitialized ||
                _storeController == null)
            {
                const string message =
                    "IAP is not initialized.";

                Debug.LogWarning(
                    $"[IAP] {message}"
                );

                PurchaseFailed?.Invoke(
                    productId,
                    message
                );

                return;
            }

            _lastRequestedProductId =
                productId;

            Debug.Log(
                $"[IAP] Purchase requested: {productId}"
            );

            _storeController.PurchaseProduct(
                productId
            );
        }

        private void SubscribeEvents()
        {
            _storeController.OnStoreConnected +=
                OnStoreConnected;

            _storeController.OnStoreDisconnected +=
                OnStoreDisconnected;

            _storeController.OnProductsFetched +=
                OnProductsFetched;

            _storeController.OnProductsFetchFailed +=
                OnProductsFetchFailed;

            _storeController.OnPurchasesFetched +=
                OnPurchasesFetched;

            _storeController.OnPurchasesFetchFailed +=
                OnPurchasesFetchFailed;

            _storeController.OnPurchasePending +=
                OnPurchasePending;

            _storeController.OnPurchaseConfirmed +=
                OnPurchaseConfirmed;

            _storeController.OnPurchaseDeferred +=
                OnPurchaseDeferred;

            _storeController.OnPurchaseFailed +=
                OnPurchaseFailed;
        }

        private void OnStoreConnected()
        {
            Debug.Log(
                "[IAP] Store connected successfully."
            );

            var products =
                new List<ProductDefinition>
                {
                    new ProductDefinition(
                        IAPProductIds.CoinsPackSmall,
                        ProductType.Consumable
                    )
                };

            _storeController.FetchProducts(
                products
            );
        }

        private void OnStoreDisconnected(
            StoreConnectionFailureDescription failure)
        {
            IsInitialized = false;

            string message =
                failure.ToString();

            Debug.LogWarning(
                $"[IAP] Store disconnected: {message}"
            );

            HandleInitializationFailure(
                message
            );
        }

        private void OnProductsFetched(
            List<Product> products)
        {
            foreach (Product product in products)
            {
                Debug.Log(
                    $"[IAP] Product fetched: " +
                    $"{product.definition.id}"
                );
            }

            IsInitialized = true;

            Debug.Log(
                "[IAP] Initialization completed."
            );

            _storeController.FetchPurchases();
        }

        private void OnProductsFetchFailed(
            ProductFetchFailed failure)
        {
            IsInitialized = false;

            HandleInitializationFailure(
                failure.ToString()
            );
        }

        private void OnPurchasesFetched(
            Orders orders)
        {
            Debug.Log(
                "[IAP] Purchases fetched."
            );
        }

        private void OnPurchasesFetchFailed(
            PurchasesFetchFailureDescription failure)
        {
            Debug.LogWarning(
                $"[IAP] Purchases fetch failed: {failure}"
            );
        }

        private void OnPurchasePending(
            PendingOrder order)
        {
            foreach (
                var item
                in order.CartOrdered.Items())
            {
                string productId =
                    item.Product.definition.id;

                Debug.Log(
                    $"[IAP] Purchase succeeded: {productId}"
                );

                PurchaseSucceeded?.Invoke(
                    productId
                );
            }

            _storeController.ConfirmPurchase(
                order
            );
        }

        private void OnPurchaseConfirmed(
            Order order)
        {
            Debug.Log(
                "[IAP] Purchase confirmed successfully."
            );
        }

        private void OnPurchaseDeferred(
            DeferredOrder order)
        {
            Debug.LogWarning(
                "[IAP] Purchase deferred. " +
                "Waiting for store approval/payment."
            );
        }

        private void OnPurchaseFailed(
            FailedOrder order)
        {
            string productId =
                string.IsNullOrEmpty(
                    _lastRequestedProductId)
                    ? "unknown"
                    : _lastRequestedProductId;

            string reason =
                order.ToString();

            Debug.LogWarning(
                $"[IAP] Purchase failed: " +
                $"{productId} | {reason}"
            );

            PurchaseFailed?.Invoke(
                productId,
                reason
            );
        }

        private void HandleInitializationFailure(
            string message)
        {
            IsInitialized = false;

            Debug.LogError(
                $"[IAP] Initialization failed: {message}"
            );

            InitializationFailed?.Invoke(
                message
            );
        }
    }
}