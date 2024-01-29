using System;
using System.Collections.Generic;

using CMS.IO;

namespace CMS.Helpers
{
    /// <summary>
    /// Mime type helper.
    /// </summary>
    public static class MimeTypeHelper
    {
        private static Dictionary<string, string> mMimetypeToExtension;
        private static Dictionary<string, string> mExtensionToMimeType;
        private static bool dataLoaded;
        private static readonly object locker = new object();


        /// <summary>
        /// Gets the extension by mime type.
        /// </summary>
        /// <param name="mimetype">Mimetype</param>
        /// <param name="defaultValue">Default value when extension is not found</param>
        public static string GetExtension(string mimetype, string defaultValue = "")
        {
            if (mimetype == null)
            {
                return defaultValue;
            }

            LoadMimeTypesInternal();

            string result;
            if (!mMimetypeToExtension.TryGetValue(mimetype, out result))
            {
                result = defaultValue;
            }

            if (result != defaultValue)
            {
                result = "." + result;
            }

            return result;
        }


        /// <summary>
        /// Gets the mimetype by extension.
        /// </summary>
        /// <param name="extension">Extension</param>
        /// <param name="defaultValue">Default value when extension is not found</param>
        public static string GetMimetype(string extension, string defaultValue = "application/octet-stream")
        {
            if (extension == null)
            {
                return defaultValue;
            }

            LoadMimeTypesInternal();

            string mimetype;
            if (!mExtensionToMimeType.TryGetValue(RemoveLeadingDotFromExtension(extension), out mimetype))
            {
                mimetype = defaultValue;
            }

            return mimetype;
        }


        /// <summary>
        /// Adds the conversion rule to the conversion tables.
        /// </summary>
        /// <param name="mimetype">Mime type</param>
        /// <param name="extension">Extension</param>
        public static void AddRule(string mimetype, string extension)
        {
            LoadMimeTypesInternal();

            lock (locker)
            {
                Dictionary<string, string> mimesToExt = new Dictionary<string, string>(mMimetypeToExtension, StringComparer.OrdinalIgnoreCase);
                Dictionary<string, string> extToMimes = new Dictionary<string, string>(mExtensionToMimeType, StringComparer.OrdinalIgnoreCase);

                var normalizedExtension = RemoveLeadingDotFromExtension(extension);
                mimesToExt[mimetype] = normalizedExtension;
                extToMimes[normalizedExtension] = mimetype;

                mMimetypeToExtension = mimesToExt;
                mExtensionToMimeType = extToMimes;
            }
        }


        /// <summary>
        /// Returns a MIME type for the given <paramref name="fileName"/>.
        /// If the <paramref name="fileName"/> indicates an image, returns a common MIME type related to file's extension; otherwise the given <paramref name="contentType"/> is returned.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="contentType">File content type</param>
        /// <returns>MIME type</returns>
        /// <remarks>
        /// Use the method to check uploaded image files to prevent XSS attacks when the file seems to be an image (according to an extension) but actually it contains HTML code
        /// including script. This method prevents a potential attacker from forging the request content type when being uploaded to the server. 
        /// </remarks>
        public static string GetSafeMimeType(string fileName, string contentType)
        {
            string extension = Path.GetExtension(fileName);

            if (ImageHelper.IsImage(extension) && !ImageHelper.IsMimeImage(contentType))
            {
                return GetMimetype(extension);
            }

            return contentType;
        }


        private static void LoadMimeTypesInternal()
        {
            if (!dataLoaded)
            {
                lock (locker)
                {
                    if (!dataLoaded)
                    {
                        Dictionary<string, string> mimesToExt = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        Dictionary<string, string> extToMimes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        PopulateMimeTypeDictionaries(ref mimesToExt, ref extToMimes);

                        mMimetypeToExtension = mimesToExt;
                        mExtensionToMimeType = extToMimes;

                        dataLoaded = true;
                    }
                }
            }
        }


        private static void PopulateMimeTypeDictionaries(ref Dictionary<string, string> mimesToExt, ref Dictionary<string, string> extToMimes)
        {
            string [][] mimetypes =
            {
                new [] { "3gp", "video/3gpp" },
                new [] { "chm", "application/octet-stream" },
                new [] { "clp", "application/x-msclip" },
                new [] { "cmx", "image/x-cmx" },
                new [] { "cnf", "text/plain" },
                new [] { "cod", "image/cis-cod" },
                new [] { "crd", "application/x-mscardfile" },
                new [] { "crl", "application/pkix-crl" },
                new [] { "crt", "application/x-x509-ca-cert" },
                new [] { "cur", "application/octet-stream" },
                new [] { "deploy", "application/octet-stream" },
                new [] { "der", "application/x-x509-ca-cert" },
                new [] { "dib", "image/bmp" },
                new [] { "disco", "text/xml" },
                new [] { "dll.config", "text/xml" },
                new [] { "dlm", "text/dlm" },
                new [] { "dsp", "application/octet-stream" },
                new [] { "dtd", "text/xml" },
                new [] { "dwf", "drawing/x-dwf" },
                new [] { "dwp", "application/octet-stream" },
                new [] { "eml", "message/rfc822" },
                new [] { "emz", "application/octet-stream" },
                new [] { "eot", "application/octet-stream" },
                new [] { "evy", "application/envoy" },
                new [] { "exe.config", "text/xml" },
                new [] { "fdf", "application/vnd.fdf" },
                new [] { "fif", "application/fractals" },
                new [] { "fla", "application/octet-stream" },
                new [] { "flr", "x-world/x-vrml" },
                new [] { "hdml", "text/x-hdml" },
                new [] { "hhc", "application/x-oleobject" },
                new [] { "hhk", "application/octet-stream" },
                new [] { "hhp", "application/octet-stream" },
                new [] { "hlp", "application/winhlp" },
                new [] { "hta", "application/hta" },
                new [] { "htc", "text/x-component" },
                new [] { "htt", "text/webviewhtml" },
                new [] { "hxt", "text/html" },
                new [] { "iii", "application/x-iphone" },
                new [] { "inf", "application/octet-stream" },
                new [] { "ins", "application/x-internet-signup" },
                new [] { "isp", "application/x-internet-signup" },
                new [] { "IVF", "video/x-ivf" },
                new [] { "jck", "application/liquidmotion" },
                new [] { "jcz", "application/liquidmotion" },
                new [] { "jfif", "image/pjpeg" },
                new [] { "jpb", "application/octet-stream" },
                new [] { "jsx", "text/jscript" },
                new [] { "lit", "application/x-ms-reader" },
                new [] { "lpk", "application/octet-stream" },
                new [] { "lsf", "video/x-la-asf" },
                new [] { "lsx", "video/x-la-asf" },
                new [] { "m13", "application/x-msmediaview" },
                new [] { "m14", "application/x-msmediaview" },
                new [] { "m1v", "video/mpeg" },
                new [] { "manifest", "application/x-ms-manifest" },
                new [] { "map", "text/plain" },
                new [] { "mdb", "application/x-msaccess" },
                new [] { "mdp", "application/octet-stream" },
                new [] { "mht", "message/rfc822" },
                new [] { "mhtml", "message/rfc822" },
                new [] { "mix", "application/octet-stream" },
                new [] { "mmf", "application/x-smaf" },
                new [] { "mno", "text/xml" },
                new [] { "mny", "application/x-msmoney" },
                new [] { "mpa", "video/mpeg" },
                new [] { "mpp", "application/vnd.ms-project" },
                new [] { "mpv2", "video/mpeg" },
                new [] { "msi", "application/octet-stream" },
                new [] { "mso", "application/octet-stream" },
                new [] { "mvb", "application/x-msmediaview" },
                new [] { "mvc", "application/x-miva-compiled" },
                new [] { "nsc", "video/x-ms-asf" },
                new [] { "nws", "message/rfc822" },
                new [] { "ocx", "application/octet-stream" },
                new [] { "odc", "text/x-ms-odc" },
                new [] { "ods", "application/oleobject" },
                new [] { "one", "application/onenote" },
                new [] { "onea", "application/onenote" },
                new [] { "onetoc", "application/onenote" },
                new [] { "onetoc2", "application/onenote" },
                new [] { "onetmp", "application/onenote" },
                new [] { "onepkg", "application/onenote" },
                new [] { "osdx", "application/opensearchdescription+xml" },
                new [] { "p10", "application/pkcs10" },
                new [] { "p12", "application/x-pkcs12" },
                new [] { "p7b", "application/x-pkcs7-certificates" },
                new [] { "p7c", "application/pkcs7-mime" },
                new [] { "p7m", "application/pkcs7-mime" },
                new [] { "p7r", "application/x-pkcs7-certreqresp" },
                new [] { "p7s", "application/pkcs7-signature" },
                new [] { "pcx", "application/octet-stream" },
                new [] { "pcz", "application/octet-stream" },
                new [] { "pfb", "application/octet-stream" },
                new [] { "pfm", "application/octet-stream" },
                new [] { "pfx", "application/x-pkcs12" },
                new [] { "pko", "application/vnd.ms-pki.pko" },
                new [] { "pma", "application/x-perfmon" },
                new [] { "pmc", "application/x-perfmon" },
                new [] { "pml", "application/x-perfmon" },
                new [] { "pmr", "application/x-perfmon" },
                new [] { "pmw", "application/x-perfmon" },
                new [] { "pnz", "image/png" },
                new [] { "pot", "application/vnd.ms-powerpoint" },
                new [] { "pps", "application/vnd.ms-powerpoint" },
                new [] { "prf", "application/pics-rules" },
                new [] { "prm", "application/octet-stream" },
                new [] { "prx", "application/octet-stream" },
                new [] { "psd", "application/octet-stream" },
                new [] { "psm", "application/octet-stream" },
                new [] { "psp", "application/octet-stream" },
                new [] { "pub", "application/x-mspublisher" },
                new [] { "qtl", "application/x-quicktimeplayer" },
                new [] { "qxd", "application/octet-stream" },
                new [] { "rar", "application/octet-stream" },
                new [] { "rf", "image/vnd.rn-realflash" },
                new [] { "rmi", "audio/mid" },
                new [] { "scd", "application/x-msschedule" },
                new [] { "sct", "text/scriptlet" },
                new [] { "sea", "application/octet-stream" },
                new [] { "setpay", "application/set-payment-initiation" },
                new [] { "setreg", "application/set-registration-initiation" },
                new [] { "sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12" },
                new [] { "sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide" },
                new [] { "smd", "audio/x-smd" },
                new [] { "smx", "audio/x-smd" },
                new [] { "smz", "audio/x-smd" },
                new [] { "snp", "application/octet-stream" },
                new [] { "spc", "application/x-pkcs7-certificates" },
                new [] { "ssm", "application/streamingmedia" },
                new [] { "sst", "application/vnd.ms-pki.certstore" },
                new [] { "stl", "application/vnd.ms-pki.stl" },
                new [] { "thmx", "application/vnd.ms-officetheme" },
                new [] { "thn", "application/octet-stream" },
                new [] { "toc", "application/octet-stream" },
                new [] { "trm", "application/x-msterminal" },
                new [] { "ttf", "application/octet-stream" },
                new [] { "u32", "application/octet-stream" },
                new [] { "uls", "text/iuls" },
                new [] { "vdx", "application/vnd.ms-visio.viewer" },
                new [] { "vml", "text/xml" },
                new [] { "vss", "application/vnd.visio" },
                new [] { "vst", "application/vnd.visio" },
                new [] { "vsto", "application/x-ms-vsto" },
                new [] { "vsw", "application/vnd.visio" },
                new [] { "vsx", "application/vnd.visio" },
                new [] { "vtx", "application/vnd.visio" },
                new [] { "wcm", "application/vnd.ms-works" },
                new [] { "wdb", "application/vnd.ms-works" },
                new [] { "wks", "application/vnd.ms-works" },
                new [] { "wmf", "application/x-msmetafile" },
                new [] { "wmp", "video/x-ms-wmp" },
                new [] { "wps", "application/vnd.ms-works" },
                new [] { "wri", "application/x-mswrite" },
                new [] { "wrz", "x-world/x-vrml" },
                new [] { "wsdl", "text/xml" },
                new [] { "x", "application/directx" },
                new [] { "xaf", "x-world/x-vrml" },
                new [] { "xaml", "application/xaml+xml" },
                new [] { "xap", "application/x-silverlight-app" },
                new [] { "xbap", "application/x-ms-xbap" },
                new [] { "xdr", "text/plain" },
                new [] { "xla", "application/vnd.ms-excel" },
                new [] { "xlc", "application/vnd.ms-excel" },
                new [] { "xlm", "application/vnd.ms-excel" },
                new [] { "xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12" },
                new [] { "xlw", "application/vnd.ms-excel" },
                new [] { "xof", "x-world/x-vrml" },
                new [] { "xps", "application/vnd.ms-xpsdocument" },
                new [] { "xsd", "text/xml" },
                new [] { "xsf", "text/xml" },
                new [] { "xslt", "text/xml" },
                new [] { "xsn", "application/octet-stream" },
                new [] { "xtp", "application/octet-stream" },

                // For backward compatibility reasons the original mimetypes overwrite the new ones
                new [] { "ai", "application/postscript" },
                new [] { "aif", "audio/x-aiff" },
                new [] { "aifc", "audio/x-aiff" },
                new [] { "aiff", "audio/x-aiff" },
                new [] { "asc", "application/pgp" },
                new [] { "asf", "video/x-ms-asf" },
                new [] { "asx", "video/x-ms-asf" },
                new [] { "au", "audio/basic" },
                new [] { "avi", "video/x-msvideo" },
                new [] { "bcpio", "application/x-bcpio" },
                new [] { "bin", "application/octet-stream" },
                new [] { "bmp", "image/bmp" },
                new [] { "c", "text/plain" },
                new [] { "c++", "text/plain" },
                new [] { "cc", "text/plain" },
                new [] { "cs", "text/plain" },
                new [] { "cpp", "text/x-c++src" },
                new [] { "cxx", "text/x-c++src" },
                new [] { "cdf", "application/x-netcdf" },
                new [] { "class", "application/octet-stream" },
                new [] { "com", "application/octet-stream" },
                new [] { "cpio", "application/x-cpio" },
                new [] { "cpt", "application/mac-compactpro" },
                new [] { "csh", "application/x-csh" },
                new [] { "css", "text/css" },
                new [] { "csv", "text/comma-separated-values" },
                new [] { "dcr", "application/x-director" },
                new [] { "diff", "text/diff" },
                new [] { "dir", "application/x-director" },
                new [] { "dll", "application/octet-stream" },
                new [] { "dms", "application/octet-stream" },
                new [] { "doc", "application/msword" },
                new [] { "docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                new [] { "dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template" },
                new [] { "docm", "application/vnd.ms-word.document.macroEnabled.12" },
                new [] { "dotm", "application/vnd.ms-word.template.macroEnabled.12" },
                new [] { "dot", "application/msword" },
                new [] { "dvi", "application/x-dvi" },
                new [] { "dxr", "application/x-director" },
                new [] { "eps", "application/postscript" },
                new [] { "etx", "text/x-setext" },
                new [] { "exe", "application/octet-stream" },
                new [] { "ez", "application/andrew-inset" },
                new [] { "flac", "audio/flac" },
                new [] { "flv", "video/x-flv" },
                new [] { "gif", "image/gif" },
                new [] { "gtar", "application/x-gtar" },
                new [] { "gz", "application/x-gzip" },
                new [] { "h", "text/plain" },
                new [] { "h++", "text/plain" },
                new [] { "hh", "text/plain" },
                new [] { "hpp", "text/plain" },
                new [] { "hxx", "text/plain" },
                new [] { "hdf", "application/x-hdf" },
                new [] { "hqx", "application/mac-binhex40" },
                new [] { "htm", "text/html" },
                new [] { "html", "text/html" },
                new [] { "ice", "x-conference/x-cooltalk" },
                new [] { "ico", "image/x-icon" },
                new [] { "ics", "text/calendar" },
                new [] { "ief", "image/ief" },
                new [] { "ifb", "text/calendar" },
                new [] { "iges", "model/iges" },
                new [] { "igs", "model/iges" },
                new [] { "jar", "application/x-jar" },
                new [] { "java", "text/x-java-source" },
                new [] { "jpe", "image/jpeg" },
                new [] { "jpeg", "image/jpeg" },
                new [] { "jpg", "image/jpeg" },
                new [] { "js", "application/x-javascript" },
                new [] { "kar", "audio/midi" },
                new [] { "latex", "application/x-latex" },
                new [] { "lha", "application/octet-stream" },
                new [] { "log", "text/plain" },
                new [] { "lzh", "application/octet-stream" },
                new [] { "m3u", "audio/x-mpegurl" },
                new [] { "man", "application/x-troff-man" },
                new [] { "me", "application/x-troff-me" },
                new [] { "mesh", "model/mesh" },
                new [] { "mid", "audio/midi" },
                new [] { "midi", "audio/midi" },
                new [] { "mif", "application/vnd.mif" },
                new [] { "mov", "video/quicktime" },
                new [] { "movie", "video/x-sgi-movie" },
                new [] { "mp2", "audio/mpeg" },
                new [] { "mp3", "audio/mpeg" },
                new [] { "mp4", "video/mp4" },
                new [] { "mpe", "video/mpeg" },
                new [] { "mpeg", "video/mpeg" },
                new [] { "mpg", "video/mpeg" },
                new [] { "mpga", "audio/mpeg" },
                new [] { "ms", "application/x-troff-ms" },
                new [] { "msh", "model/mesh" },
                new [] { "mxu", "video/vnd.mpegurl" },
                new [] { "nc", "application/x-netcdf" },
                new [] { "oda", "application/oda" },
                new [] { "patch", "text/diff" },
                new [] { "pbm", "image/x-portable-bitmap" },
                new [] { "pdb", "chemical/x-pdb" },
                new [] { "pdf", "application/pdf" },
                new [] { "pgm", "image/x-portable-graymap" },
                new [] { "pgn", "application/x-chess-pgn" },
                new [] { "pgp", "application/pgp" },
                new [] { "php", "application/x-httpd-php" },
                new [] { "php3", "application/x-httpd-php3" },
                new [] { "pl", "application/x-perl" },
                new [] { "pm", "application/x-perl" },
                new [] { "png", "image/png" },
                new [] { "pnm", "image/x-portable-anymap" },
                new [] { "po", "text/plain" },
                new [] { "ps", "application/postscript" },
                new [] { "potm", "application/vnd.ms-powerpoint.template.macroEnabled.12" },
                new [] { "potx", "application/vnd.openxmlformats-officedocument.presentationml.template" },
                new [] { "ppm", "image/x-portable-pixmap" },
                new [] { "ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow" },
                new [] { "ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12" },
                new [] { "ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12" },
                new [] { "ppt", "application/vnd.ms-powerpoint" },
                new [] { "pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12" },
                new [] { "pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                new [] { "qt", "video/quicktime" },
                new [] { "ra", "audio/x-realaudio" },
                new [] { "ram", "audio/x-pn-realaudio" },
                new [] { "ras", "image/x-cmu-raster" },
                new [] { "rgb", "image/x-rgb" },
                new [] { "rm", "audio/x-pn-realaudio" },
                new [] { "roff", "application/x-troff" },
                new [] { "rpm", "audio/x-pn-realaudio-plugin" },
                new [] { "rtf", "text/rtf" },
                new [] { "rtx", "text/richtext" },
                new [] { "sgm", "text/sgml" },
                new [] { "sgml", "text/sgml" },
                new [] { "sh", "application/x-sh" },
                new [] { "shar", "application/x-shar" },
                new [] { "shtml", "text/html" },
                new [] { "silo", "model/mesh" },
                new [] { "sit", "application/x-stuffit" },
                new [] { "skd", "application/x-koan" },
                new [] { "skm", "application/x-koan" },
                new [] { "skp", "application/x-koan" },
                new [] { "skt", "application/x-koan" },
                new [] { "smi", "application/smil" },
                new [] { "smil", "application/smil" },
                new [] { "snd", "audio/basic" },
                new [] { "so", "application/octet-stream" },
                new [] { "spl", "application/x-futuresplash" },
                new [] { "src", "application/x-wais-source" },
                new [] { "stc", "application/vnd.sun.xml.calc.template" },
                new [] { "std", "application/vnd.sun.xml.draw.template" },
                new [] { "sti", "application/vnd.sun.xml.impress.template" },
                new [] { "stw", "application/vnd.sun.xml.writer.template" },
                new [] { "sv4cpio", "application/x-sv4cpio" },
                new [] { "sv4crc", "application/x-sv4crc" },
                new [] { "swf", "application/x-shockwave-flash" },
                new [] { "sxc", "application/vnd.sun.xml.calc" },
                new [] { "sxd", "application/vnd.sun.xml.draw" },
                new [] { "sxg", "application/vnd.sun.xml.writer.global" },
                new [] { "sxi", "application/vnd.sun.xml.impress" },
                new [] { "sxm", "application/vnd.sun.xml.math" },
                new [] { "sxw", "application/vnd.sun.xml.writer" },
                new [] { "t", "application/x-troff" },
                new [] { "tar", "application/x-tar" },
                new [] { "tcl", "application/x-tcl" },
                new [] { "tex", "application/x-tex" },
                new [] { "texi", "application/x-texinfo" },
                new [] { "texinfo", "application/x-texinfo" },
                new [] { "tgz", "application/x-gtar" },
                new [] { "tif", "image/tiff" },
                new [] { "tiff", "image/tiff" },
                new [] { "tr", "application/x-troff" },
                new [] { "tsv", "text/tab-separated-values" },
                new [] { "ustar", "application/x-ustar" },
                new [] { "vbs", "text/plain" },
                new [] { "vcd", "application/x-cdlink" },
                new [] { "vcf", "text/x-vcard" },
                new [] { "vcs", "text/calendar" },
                new [] { "vfb", "text/calendar" },
                new [] { "vrml", "model/vrml" },
                new [] { "vsd", "application/vnd.visio" },
                new [] { "wav", "audio/x-wav" },
                new [] { "wax", "audio/x-ms-wax" },
                new [] { "wbmp", "image/vnd.wap.wbmp" },
                new [] { "wbxml", "application/vnd.wap.wbxml" },
                new [] { "wm", "video/x-ms-wm" },
                new [] { "wma", "audio/x-ms-wma" },
                new [] { "wmd", "application/x-ms-wmd" },
                new [] { "wml", "text/vnd.wap.wml" },
                new [] { "wmlc", "application/vnd.wap.wmlc" },
                new [] { "wmls", "text/vnd.wap.wmlscript" },
                new [] { "wmlsc", "application/vnd.wap.wmlscriptc" },
                new [] { "wmv", "video/x-ms-wmv" },
                new [] { "wmx", "video/x-ms-wmx" },
                new [] { "wmz", "application/x-ms-wmz" },
                new [] { "wrl", "model/vrml" },
                new [] { "wvx", "video/x-ms-wvx" },
                new [] { "xbm", "image/x-xbitmap" },
                new [] { "xht", "application/xhtml+xml" },
                new [] { "xhtml", "application/xhtml+xml" },
                new [] { "xlam", "application/vnd.ms-excel.addin.macroEnabled.12" },
                new [] { "xls", "application/vnd.ms-excel" },
                new [] { "xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12" },
                new [] { "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                new [] { "xlt", "application/vnd.ms-excel" },
                new [] { "xltm", "application/vnd.ms-excel.template.macroEnabled.12" },
                new [] { "xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template" },
                new [] { "xml", "application/xml" },
                new [] { "xpm", "image/x-xpixmap" },
                new [] { "xsl", "text/xml" },
                new [] { "xwd", "image/x-xwindowdump" },
                new [] { "xyz", "chemical/x-xyz" },
                new [] { "z", "application/x-compress" },
                new [] { "zip", "application/zip" },
                new [] { "webm", "video/webm" },
                new [] { "ogg", "video/ogg" },
                new [] { "oga", "audio/ogg" },
                new [] { "ogv", "video/ogg" },
                new [] { "woff", "application/font-woff" },
                new [] { "txt", "text/plain" },
                new [] { "svg", "image/svg+xml" }
            };

            foreach (var mimePair in mimetypes)
            {
                extToMimes[mimePair[0]] = mimePair[1];
                mimesToExt[mimePair[1]] = mimePair[0];
            }
        }


        private static string RemoveLeadingDotFromExtension(string extension)
        {
            if (String.IsNullOrEmpty(extension))
            {
                return extension;
            }

            return extension.TrimStart('.');
        }
    }
}