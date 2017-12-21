# Crawly

A prototype attempt at making a web crawler in .net. It uses HtmlAgilityPack to view the html.

Crawly is the crawling library and CrawlyRunner is a sample program showing how to use it. CrawlyRunner is looking for RSS feeds, starting with some podcast host's sites.

There are some perf problems currently, after a while HttpWebRequest gets very slow. I'm considering replacing it, since this isn't something I'm actually using I may never replace it.
