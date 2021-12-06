﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Braintree;

namespace BraintreeASPExample.Controllers
{
    public class HostedController : Controller
    {
        public IBraintreeConfiguration config = new BraintreeConfiguration();

        public static readonly TransactionStatus[] transactionSuccessStatuses = {
            TransactionStatus.AUTHORIZED,
            TransactionStatus.AUTHORIZING,
            TransactionStatus.SETTLED,
            TransactionStatus.SETTLING,
            TransactionStatus.SETTLEMENT_CONFIRMED,
            TransactionStatus.SETTLEMENT_PENDING,
            TransactionStatus.SUBMITTED_FOR_SETTLEMENT
        };

        public ActionResult New()
        {
            var gateway = config.GetGateway();
            var clientToken = gateway.ClientToken.Generate();
            ViewBag.ClientToken = clientToken;
            return View();
        }

        public ActionResult Create()
        {
            var gateway = config.GetGateway();

            decimal amount;
            try
            {
                amount = decimal.Parse(Request["amount"]);
            }
            catch
            {
                var errorMessages = "Unable to read Amount as a Decimal : " + Request["amount"];
                TempData["Flash"] = errorMessages;
                return RedirectToAction("New");
            }

            var nonce = Request["payment_method_nonce"];

            if (string.IsNullOrEmpty(nonce))
            {
                var errorMessages = "Payment Nonce not submitted";
                TempData["Flash"] = errorMessages;
                return RedirectToAction("New");
            }

            var customer = new CustomerRequest()
            {
                FirstName = "Test1",
                LastName = "LName",
                Email = "test1@lname.com"
            };

            var request = new TransactionRequest
            {
                Amount = amount,
                PaymentMethodNonce = nonce,
                Customer = customer,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true,
                    StoreInVaultOnSuccess = true,
                }
            };

            var result = gateway.Transaction.Sale(request);
            if (result.IsSuccess())
            {
                Transaction transaction = result.Target;
                return RedirectToAction("Index", "Show", new { id = transaction.Id });
            }
            else if (result.Transaction != null)
            {
                return RedirectToAction("Index", "Show", new { id = result.Transaction.Id });
            }
            else
            {
                var errorMessages = "";
                foreach (var error in result.Errors.DeepAll())
                {
                    errorMessages += "Error: " + (int)error.Code + " - " + error.Message + "\n";
                }
                TempData["Flash"] = errorMessages;
                return RedirectToAction("New");
            }
        }
    }
}
