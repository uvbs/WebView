﻿using System.Security.Cryptography.X509Certificates;
using CefSharp;

namespace WebViewControl {

    partial class WebView {

        private class CefRequestHandler : IRequestHandler {

            private readonly WebView OwnerWebView;

            public CefRequestHandler(WebView webView) {
                OwnerWebView = webView;
            }

            bool IRequestHandler.OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture) {
                return false;
            }

            bool IRequestHandler.OnProtocolExecution(IWebBrowser browserControl, IBrowser browser, string url) {
                return false;
            }

            bool IRequestHandler.OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, string originUrl, long newSize, IRequestCallback callback) {
                return false;
            }

            void IRequestHandler.OnRenderViewReady(IWebBrowser browserControl, IBrowser browser) {
            }

            void IRequestHandler.OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength) {
            }

            bool IRequestHandler.OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response) {
                return false;
            }

            bool IRequestHandler.GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, IFrame frame, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback) {
                if (OwnerWebView.ProxyAuthentication != null) {
                    callback.Continue(OwnerWebView.ProxyAuthentication.UserName, OwnerWebView.ProxyAuthentication.Password);
                }
                return true;
            }

            IResponseFilter IRequestHandler.GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response) {
                return null;
            }

            bool IRequestHandler.OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool isRedirect) {
                if (FilterRequest(request)) {
                    return false;
                }
                
                if (OwnerWebView.IsHistoryDisabled && (request.TransitionType & TransitionType.ForwardBack) == TransitionType.ForwardBack) {
                    return true;
                }
               
                bool cancel = false;
                if (OwnerWebView.BeforeNavigate != null) {
                    var wrappedRequest = new Request(request, OwnerWebView.GetRequestUrl(request));
                    OwnerWebView.ExecuteWithAsyncErrorHandling(() => OwnerWebView.BeforeNavigate(wrappedRequest));
                    cancel = wrappedRequest.Canceled;
                }

                return cancel;
            }

            CefReturnValue IRequestHandler.OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback) { 
                return CefReturnValue.Continue;
            }

            bool IRequestHandler.OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback) {
                if (OwnerWebView.IgnoreCertificateErrors) {
                    callback.Continue(true);
                    return true;
                }
                return false;
            }

            void IRequestHandler.OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, string pluginPath) {
            }

            void IRequestHandler.OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status) {
                OwnerWebView.RenderProcessCrashed?.Invoke();
                string reason = "";
                bool wasKilled = false;
                switch (status) {
                    case CefTerminationStatus.AbnormalTermination:
                        reason = "terminated with an unknown reason";
                        break;
                    case CefTerminationStatus.ProcessCrashed:
                        reason = "crashed";
                        break;
                    case CefTerminationStatus.ProcessWasKilled:
                        reason = "was killed";
                        wasKilled = true;
                        break;
                }
                OwnerWebView.ExecuteWithAsyncErrorHandling(() => throw new RenderProcessTerminatedException("WebView render process " + reason, wasKilled));
            }

            bool IRequestHandler.OnSelectClientCertificate(IWebBrowser browserControl, IBrowser browser, bool isProxy, string host, int port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback) {
                return false;
            }

            void IRequestHandler.OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl) {
            }
        }
    }
}
