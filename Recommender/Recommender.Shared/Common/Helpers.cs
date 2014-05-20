//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.Web;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;

namespace Recommender.Common
{
    internal static class Helpers
    {
        internal static string GetJosnObjectIntegerValue(JsonObject jsonObject, string key)
        {
            string reValue = "";
            if ((jsonObject == null) || string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                return reValue;
            }

            if (jsonObject.ContainsKey(key))
            {
                double value = jsonObject.GetNamedNumber(key);
                reValue = Convert.ToString((int)value);
            }
            return reValue;
        }

        internal static string GetJosnObjectStringValue(JsonObject jsonObject, string key)
        {
            string reValue = "";
            if ((jsonObject == null) || string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                return reValue;
            }

            if (jsonObject.ContainsKey(key))
            {
                string value = jsonObject.GetNamedString(key); ;
                if ((string.IsNullOrEmpty(value)) || (value.Equals("null")))
                {
                    return reValue;
                }
                else
                {
                    value.TrimStart();
                    value.TrimEnd();

                    reValue = value;
                }
            }
            return reValue;
        }

        internal static async void InternetFailureAlarm()
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            string connectionFailureAlarm = loader.GetString("ConnectionFailureAlarm");
            string confirmStr = loader.GetString("Confirm");

            // Create the message dialog and set its content
            var messageDialog = new MessageDialog(connectionFailureAlarm);

            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
            messageDialog.Commands.Add(new UICommand(confirmStr));

            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 0;

            // Show the message dialog
            await messageDialog.ShowAsync();
        }

        internal static async Task<string> GetResultAsync(
            HttpResponseMessage response,
            CancellationToken token)
        {
            string responseBodyAsText;
            //output += SerializeHeaders(response);
            SerializeHeaders(response);
            responseBodyAsText = await response.Content.ReadAsStringAsync().AsTask(token);

            token.ThrowIfCancellationRequested();

            // Insert new lines.
            responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine);

            return responseBodyAsText;
        }

        internal static async Task DisplayTextResultAsync(
            HttpResponseMessage response,
            TextBox output,
            CancellationToken token)
        {
            string responseBodyAsText;
            output.Text += SerializeHeaders(response);
            responseBodyAsText = await response.Content.ReadAsStringAsync().AsTask(token);

            token.ThrowIfCancellationRequested();

            // Insert new lines.
            responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine);

            output.Text += responseBodyAsText;
        }

        internal static string SerializeHeaders(HttpResponseMessage response)
        {
            StringBuilder output = new StringBuilder();

            // We cast the StatusCode to an int so we display the numeric value (e.g., "200") rather than the
            // name of the enum (e.g., "OK") which would often be redundant with the ReasonPhrase.
            output.Append(((int)response.StatusCode) + " " + response.ReasonPhrase + "\r\n");

            SerializeHeaderCollection(response.Headers, output);
            SerializeHeaderCollection(response.Content.Headers, output);
            output.Append("\r\n");
            return output.ToString();
        }

        internal static void SerializeHeaderCollection(
            IEnumerable<KeyValuePair<string, string>> headers,
            StringBuilder output)
        {
            foreach (var header in headers)
            {
                output.Append(header.Key + ": " + header.Value + "\r\n");
            }
        }

        internal static void CreateHttpClient(ref HttpClient httpClient)
        {
            if (httpClient != null)
            {
                httpClient.Dispose();
            }

            // HttpClient functionality can be extended by plugging multiple filters together and providing
            // HttpClient with the configured filter pipeline.
            IHttpFilter filter = new HttpBaseProtocolFilter();
            filter = new PlugInFilter(filter); // Adds a custom header to every request and response message.
            httpClient = new HttpClient(filter);

            // The following line sets a "User-Agent" request header as a default header on the HttpClient instance.
            // Default headers will be sent with every request sent from this HttpClient instance.
            httpClient.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("Sample", "v8"));
        }

        internal static void ScenarioStarted(Button startButton, Button cancelButton, TextBox outputField)
        {
            startButton.IsEnabled = false;
            cancelButton.IsEnabled = true;
            if (outputField != null)
            {
                outputField.Text = String.Empty;
            }
        }

        internal static void ScenarioCompleted(Button startButton, Button cancelButton)
        {
            startButton.IsEnabled = true;
            cancelButton.IsEnabled = false;
        }

        internal static void ReplaceQueryString(TextBox addressField, string newQueryString)
        {
            string resourceAddress = addressField.Text;

            // Remove previous query string.
            int questionMarkIndex = resourceAddress.IndexOf("?", StringComparison.Ordinal);
            if (questionMarkIndex != -1)
            {
                resourceAddress = resourceAddress.Substring(0, questionMarkIndex);
            }

            addressField.Text = resourceAddress + newQueryString;
        }

        internal static bool TryGetUri(string uriString, out Uri uri)
        {
            // Note that this app has both "Internet (Client)" and "Home and Work Networking" capabilities set,
            // since the user may provide URIs for servers located on the internet or intranet. If apps only
            // communicate with servers on the internet, only the "Internet (Client)" capability should be set.
            // Similarly if an app is only intended to communicate on the intranet, only the "Home and Work
            // Networking" capability should be set.
            if (!Uri.TryCreate(uriString.Trim(), UriKind.Absolute, out uri))
            {
                return false;
            }

            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                return false;
            }

            return true;
        }
    }
}
