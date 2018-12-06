// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Microsoft.Docs.Build
{
    internal static class HrefUtility
    {
        /// <summary>
        /// Split href to path, fragement and query
        /// </summary>
        /// <param name="href">The href</param>
        /// <returns>The splited path, query and fragment</returns>
        public static (string path, string query, string fragment) SplitHref(string href)
        {
            var path = "";
            var query = "";
            var fragment = "";

            var fragmentIndex = href.IndexOf('#');
            if (fragmentIndex >= 0)
            {
                fragment = href.Substring(fragmentIndex);
                var queryIndex = href.IndexOf('?', 0, fragmentIndex);
                if (queryIndex >= 0)
                {
                    query = href.Substring(queryIndex, fragmentIndex - queryIndex);
                    path = href.Substring(0, queryIndex);
                }
                else
                {
                    path = href.Substring(0, fragmentIndex);
                }
            }
            else
            {
                var queryIndex = href.IndexOf('?');
                if (queryIndex >= 0)
                {
                    query = href.Substring(queryIndex);
                    path = href.Substring(0, queryIndex);
                }
                else
                {
                    path = href;
                }
            }

            return (path, query, fragment);
        }

        /// <summary>
        /// <paramref name="sourceQuery"/> and <paramref name="sourceFragment"/> will overwrite the ones in <paramref name="targetHref"/>
        /// </summary>
        public static string MergeHref(string targetHref, string sourceQuery, string sourceFragment)
        {
            var (targetPath, targetQuery, targetFragment) = SplitHref(targetHref);
            if (string.IsNullOrEmpty(targetPath))
                return targetHref;

            var targetQueryParameters = HttpUtility.ParseQueryString(targetQuery.Length == 0 ? "" : targetQuery.Substring(1));
            var sourceQueryParameters = HttpUtility.ParseQueryString(sourceQuery);

            foreach (var key in sourceQueryParameters.AllKeys)
            {
                targetQueryParameters.Set(key, sourceQueryParameters[key]);
            }

            var query = targetQueryParameters.HasKeys() ? "?" + targetQueryParameters.ToString() : string.Empty;
            var fragment = sourceFragment?.Length == 0 ? targetFragment : "#" + sourceFragment;

            return targetPath + query + fragment;
        }

        /// <summary>
        /// Get the git remote information from remote href
        /// </summary>
        /// <param name="remoteHref">The git remote href like https://github.com/dotnet/docfx#master</param>
        public static (string remote, string refspec) SplitGitHref(string remoteHref)
        {
            Debug.Assert(!string.IsNullOrEmpty(remoteHref));

            var (path, _, fragment) = SplitHref(remoteHref);

            var refspec = (string.IsNullOrEmpty(fragment) || fragment.Length <= 1) ? "master" : fragment.Substring(1);

            return (path, refspec);
        }

        public static DependencyType FragmentToDependencyType(string fragment)
        {
            Debug.Assert(string.IsNullOrEmpty(fragment) || fragment[0] == '#');

            return fragment != null && fragment.Length > 1 ? DependencyType.Bookmark : DependencyType.Link;
        }

        public static bool IsAbsoluteHref(string str)
        {
            return str.StartsWith('/')
                || str.StartsWith('\\')
                || Uri.TryCreate(str, UriKind.Absolute, out _);
        }

        public static bool IsHttpHref(string str)
        {
            return !string.IsNullOrEmpty(str)
                && Uri.TryCreate(str, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static string EscapeUrl(string path)
        {
            return string.Join('/', path.Split('/', '\\').Select(segment => Uri.EscapeDataString(segment)));
        }

        public static string EscapeUrlSegment(string path)
        {
            return Uri.EscapeDataString(path);
        }

        public static string UnescapeUrl(string path)
        {
            return Uri.UnescapeDataString(path);
        }
    }
}
