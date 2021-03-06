﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using i18n.Domain.Concrete;

namespace i18n.Tests
{
    [TestClass]
    public class ResponseFilterTests
    {
        void Helper_ResponseFilter_can_patch_html_urls(string suffix, string pre, string expectedPatched, Uri requestUrl = null)
        {
            i18n.EarlyUrlLocalizer obj = new i18n.EarlyUrlLocalizer(new UrlLocalizer());
            string post = obj.ProcessOutgoing(pre, suffix, null);
            Assert.AreEqual(expectedPatched, post);
        }

        [TestMethod]
        public void ResponseFilter_can_patch_html_urls()
        {
            // One attribute.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<a href=\"/\"></a>",
                "<a href=\"/fr\"></a>");

            // Two attributes.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<a href=\"/\" title=\"Home\"></a>",
                "<a href=\"/fr\" title=\"Home\"></a>");

            // Three attributes.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<a class=\"corporate_logo\" href=\"/\" title=\"Home\"></a>",
                "<a class=\"corporate_logo\" href=\"/fr\" title=\"Home\"></a>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<a class=\"corporate_logo\" href=\"/aaaa\" title=\"Home\"></a>",
                "<a class=\"corporate_logo\" href=\"/fr/aaaa\" title=\"Home\"></a>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<a class=\"corporate_logo\" title=\"Home\" href=\"/\"></a>",
                "<a class=\"corporate_logo\" title=\"Home\" href=\"/fr\"></a>");

            // Nonrelevant tag.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script1 src=\"123\"></script1>",
                "<script1 src=\"123\"></script1>");

            // script tag.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123\"></script>",
                "<script src=\"/fr/123\"></script>");

            // img tag.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<img src=\"123\"></img>",
                "<img src=\"/fr/123\"></img>");

            // a tag.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<a href=\"123\"></a>",
                "<a href=\"/fr/123\"></a>");

            // form tag.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<form action=\"123\"></form>",
                "<form action=\"/fr/123\"></form>");

            // Embedded tags.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<img src=\"123\"><a href=\"123\"></a></img>",
                "<img src=\"/fr/123\"><a href=\"/fr/123\"></a></img>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<img src=\"123\"><a href=\"123\" /></img>",
                "<img src=\"/fr/123\"><a href=\"/fr/123\" /></img>");

            // Different langtags.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123\"></script>",
                "<script src=\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr-FR",
                "<script src=\"123\"></script>",
                "<script src=\"/fr-FR/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "zh-Hans",
                "<script src=\"123\"></script>",
                "<script src=\"/zh-Hans/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "zh-Hans-HK",
                "<script src=\"123\"></script>",
                "<script src=\"/zh-Hans-HK/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "zh-Hans-HK-x-sadadssad-asdasdadad-asdsadad",
                "<script src=\"123\"></script>",
                "<script src=\"/zh-Hans-HK-x-sadadssad-asdasdadad-asdsadad/123\"></script>");

            // Relative and absolute URLs.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123\"></script>",
                "<script src=\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"/123\"></script>",
                "<script src=\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"http://example.com/123\"></script>",
                "<script src=\"http://example.com/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"https://example.com/123\"></script>",
                "<script src=\"https://example.com/fr/123\"></script>");

            // More complex paths.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123/a/b/c/d\"></script>",
                "<script src=\"/fr/123/a/b/c/d\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123/a/b/c/d.js\"></script>",
                "<script src=\"/fr/123/a/b/c/d.js\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123/a/b/c/d.X.Y.Z.js\"></script>",
                "<script src=\"/fr/123/a/b/c/d.X.Y.Z.js\"></script>");

            // Query strings.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123?a=b\"></script>",
                "<script src=\"/fr/123?a=b\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123?a=b&c=d\"></script>",
                "<script src=\"/fr/123?a=b&c=d\"></script>");

            // Single full script tag.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123\"></script>",
                "<script src=\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123 \"></script>",
                "<script src=\"/fr/123 \"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123\"></script>",
                "<script src=\" /fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \"></script>",
                "<script src=\" /fr/123 \"></script>");

            // Two full script tags.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123\"></script><script src=\"123\"></script>",
                "<script src=\"/fr/123\"></script><script src=\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\"123 \"></script><script src=\"123 \"></script>",
                "<script src=\"/fr/123 \"></script><script src=\"/fr/123 \"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123\"></script><script src=\" 123\"></script>",
                "<script src=\" /fr/123\"></script><script src=\" /fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \"></script><script src=\" 123 \"></script>",
                "<script src=\" /fr/123 \"></script><script src=\" /fr/123 \"></script>");

            // Single short script tag.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" />",
                "<script src=\" /fr/123 \" />");

            // Two short script tags.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" /><script src=\" 123 \" />",
                "<script src=\" /fr/123 \" /><script src=\" /fr/123 \" />");

            // Two short script tags separated.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" />        <script src=\" 123 \" />",
                "<script src=\" /fr/123 \" />        <script src=\" /fr/123 \" />");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" />        <script src=\" 123 \" />",
                "<script src=\" /fr/123 \" />        <script src=\" /fr/123 \" />");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" /> <<<<       <script src=\" 123 \" />",
                "<script src=\" /fr/123 \" /> <<<<       <script src=\" /fr/123 \" />");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" /> ><><>><<<><><><       <script src=\" 123 \" />",
                "<script src=\" /fr/123 \" /> ><><>><<<><><><       <script src=\" /fr/123 \" />");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" /> ><234><234>2323>243<324<4<24><>ffdsd<aadda>d<a       <script src=\" 123 \" />",
                "<script src=\" /fr/123 \" /> ><234><234>2323>243<324<4<24><>ffdsd<aadda>d<a       <script src=\" /fr/123 \" />");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=\" 123 \" /> <m> <n> </n> </m>       <script src=\" 123 \" />",
                "<script src=\" /fr/123 \" /> <m> <n> </n> </m>       <script src=\" /fr/123 \" />");

            // Script tags embedded in other tags.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<body>\n<script src=\" 123 \" /><script src=\" 123 \" /></body>",
                "<body>\n<script src=\" /fr/123 \" /><script src=\" /fr/123 \" /></body>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<body><body><body>\n<script src=\" 123 \" /><script src=\" 123 \" /></body></body></body>",
                "<body><body><body>\n<script src=\" /fr/123 \" /><script src=\" /fr/123 \" /></body></body></body>");

            // Random spaces.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src =\"123\"></script>",
                "<script src =\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src     =\"123\"></script>",
                "<script src     =\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src= \"123\"></script>",
                "<script src= \"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src=   \"123\"></script>",
                "<script src=   \"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script src = \"123\"></script>",
                "<script src = \"/fr/123\"></script>");

            // Random linefeeds.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\nsrc\n=\"123\"></script\n>",
                "<script\nsrc\n=\"/fr/123\"></script\n>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\nsrc\n=\"123\"></script>",
                "<script\nsrc\n=\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\nsrc=\n\"123\"></script>",
                "<script\nsrc=\n\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\nsrc=\n\n\n\"123\"></script>",
                "<script\nsrc=\n\n\n\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "\n<script\nsrc\n=\n\"123\"\n>\n</script\n>\n",
                "\n<script\nsrc\n=\n\"/fr/123\"\n>\n</script\n>\n");

            // Random CRLFs and tabs.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\nsrc\r\n=\"123\"></script\r\n>",
                "<script\r\nsrc\r\n=\"/fr/123\"></script\r\n>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\nsrc\r\n=\"123\"></script>",
                "<script\r\nsrc\r\n=\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\nsrc=\r\n\"123\"></script>",
                "<script\r\nsrc=\r\n\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\nsrc=\r\n\r\n\r\n\"123\"></script>",
                "<script\r\nsrc=\r\n\r\n\r\n\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "\r\n<script\r\nsrc\r\n=\r\n\"123\"\r\n>\r\n</script\r\n>\r\n",
                "\r\n<script\r\nsrc\r\n=\r\n\"/fr/123\"\r\n>\r\n</script\r\n>\r\n");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\"123\"></script\r\r\n\n\n\r\n\n\r\t\n\r\t>",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\"/fr/123\"></script\r\r\n\n\n\r\n\n\r\t\n\r\t>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\"123\"></script>",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc=\r\r\n\n\n\r\n\n\r\t\n\r\t\"123\"></script>",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc=\r\r\n\n\n\r\n\n\r\t\n\r\t\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc=\r\r\n\n\n\r\n\n\r\t\n\r\t\r\r\n\n\n\r\n\n\r\t\n\r\t\r\r\n\n\n\r\n\n\r\t\n\r\t\"123\"></script>",
                "<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc=\r\r\n\n\n\r\n\n\r\t\n\r\t\r\r\n\n\n\r\n\n\r\t\n\r\t\r\r\n\n\n\r\n\n\r\t\n\r\t\"/fr/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                "\r\r\n\n\n\r\n\n\r\t\n\r\t<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\r\r\n\n\n\r\n\n\r\t\n\r\t\"123\"\r\r\n\n\n\r\n\n\r\t\n\r\t>\r\r\n\n\n\r\n\n\r\t\n\r\t</script\r\r\n\n\n\r\n\n\r\t\n\r\t>\r\r\n\n\n\r\n\n\r\t\n\r\t",
                "\r\r\n\n\n\r\n\n\r\t\n\r\t<script\r\r\n\n\n\r\n\n\r\t\n\r\tsrc\r\r\n\n\n\r\n\n\r\t\n\r\t=\r\r\n\n\n\r\n\n\r\t\n\r\t\"/fr/123\"\r\r\n\n\n\r\n\n\r\t\n\r\t>\r\r\n\n\n\r\n\n\r\t\n\r\t</script\r\r\n\n\n\r\n\n\r\t\n\r\t>\r\r\n\n\n\r\n\n\r\t\n\r\t");

            // IGNORE_LOCALIZATION URLs.
            // These should not be changed by the filter.
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                string.Format("<script src=\"{0}123\"></script>", EarlyUrlLocalizer.IgnoreLocalizationUrlPrefix),
                "<script src=\"123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                string.Format("<script src=\"{0}/123\"></script>", EarlyUrlLocalizer.IgnoreLocalizationUrlPrefix),
                "<script src=\"/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                string.Format("<script src=\"{0}http://example.com/123\"></script>", EarlyUrlLocalizer.IgnoreLocalizationUrlPrefix),
                "<script src=\"http://example.com/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                string.Format("<script src=\"{0}https://example.com/123\"></script>", EarlyUrlLocalizer.IgnoreLocalizationUrlPrefix),
                "<script src=\"https://example.com/123\"></script>");
            Helper_ResponseFilter_can_patch_html_urls(
                "fr",
                string.Format("<script src=\"{0}https://example.com/fr/123\"></script>", EarlyUrlLocalizer.IgnoreLocalizationUrlPrefix),
                "<script src=\"https://example.com/fr/123\"></script>");
        }
    }
}
