using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Dav.AspNetCore.Server
{
    public class WebDavPath
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static WebDavPath FromString(String path)
        {
            ArgumentNullException.ThrowIfNull(path, nameof(path));

            path = normalize(path);

            return new WebDavPath(path.Length == 0 ? "/" : path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="breaker"></param>
        /// <param name="heads"></param>
        /// <param name="tails"></param>
        private static void breakInput(String input, char breaker, out String heads, out String? tails)
        {
            int index = input.IndexOf(breaker);
            if (index != -1)
            {
                heads = input.Substring(0, index);
                tails = input.Substring(index);
            }
            else
            {
                heads = input;
                tails = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static WebDavPath FromUri(Uri url)
        {
            ArgumentNullException.ThrowIfNull(url, nameof(url));

            if (url.IsAbsoluteUri)
            {
                return FromString($"{url.PathAndQuery}{url.Fragment}");
            }
            else
            {
                return FromString(url.OriginalString);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static WebDavPath Combine(WebDavPath basePath, string path)
        {
            ArgumentNullException.ThrowIfNull(basePath, nameof(basePath));
            ArgumentNullException.ThrowIfNull(path, nameof(path));

            path = normalize(path);

            var localPath = basePath.LocalPath;
            if (!localPath.EndsWith("/"))
                localPath += "/";

            return WebDavPath.FromString($"{localPath}{path.TrimStart('/')}{basePath.Query}{basePath.Fragment}");
        }


        public static bool TryCreate(String input,  out WebDavPath? path)
        {
            try
            {
                path = FromString(input);
                return true;
            }
            catch
            {
                path = null;
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static String normalize(String input)
        {
            return input.Replace('\\', '/');
        }


        /// <summary>
        /// 
        /// </summary>
        public String LocalPath { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public String[] Segments { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public String? Query { get; private set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public String? Fragment { get; private set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public String? FileName { get; private set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public bool IsAbsolutePath { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public String AbsolutePath { get => LocalPath; }

        /// <summary>
        /// 
        /// </summary>
        public void toDirectory()
        {
            if (!this.LocalPath.EndsWith('/'))
            {
                this.LocalPath += '/';
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private WebDavPath(String path)
        {
            String? heads, tails;

            breakInput(path, '#', out heads, out tails);

            this.Fragment = tails;

            breakInput(heads, '?', out heads, out tails);
            this.Query = tails;

            this.LocalPath = heads;

            String[] parts = heads.Split('/');

            int count = parts.Length;
            if (heads.EndsWith('/')) count--;

            this.Segments = new String[count];

            for (int i = 0; i < this.Segments.Length; i++)
            {
                this.Segments[i] = parts[i] + "/";
            }
            
            if (!heads.EndsWith('/'))
            {
                this.Segments[^1] = this.Segments[^1].TrimEnd('/');
                this.FileName = this.Segments[^1];
            }

            this.IsAbsolutePath = this.LocalPath.StartsWith("/");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public WebDavPath GetParent()
        {
            if (this.Segments.Length == 1)
                return WebDavPath.FromString(this.Segments[0]);

            var uriString = string.Empty;
            for (var i = 0; i < this.Segments.Length - 1; i++)
            {
                uriString += this.Segments[i];
            }

            return WebDavPath.FromString($"{uriString}{this.Query}{this.Fragment}");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="relativeTo"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public WebDavPath GetRelativePath(WebDavPath path)
        {
            ArgumentNullException.ThrowIfNull(path, nameof(path));

            if (path.Segments.Length > this.Segments.Length)
                return path;

            // validate root
            for (var i = 0; i < path.Segments.Length; i++)
            {
                if (this.Segments[i].Trim('/') != path.Segments[i].Trim('/'))
                    return path;
            }

            var relativePath = string.Join("", this.Segments.Skip(path.Segments.Length));
            if (!relativePath.StartsWith("/"))
                relativePath = $"/{relativePath}";

            if (relativePath.EndsWith("/"))
                relativePath = relativePath.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(relativePath))
                return WebDavPath.FromString("/");

            return WebDavPath.FromString($"{relativePath}{this.Query}{this.Fragment}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{this.LocalPath}{this.Query}{this.Fragment}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if(obj == null) return false;   
            return this.ToString().Equals(((WebDavPath)obj).ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public static Boolean operator ==(WebDavPath left, WebDavPath right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (ReferenceEquals(left, null))
                return false;

            return left.Equals(right);
        }

        public static Boolean operator !=(WebDavPath left, WebDavPath right)
        {
            return !(left == right);
        }
    }
}
