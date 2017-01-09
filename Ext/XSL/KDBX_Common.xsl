<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output method="xml" encoding="UTF-8" omit-xml-declaration="yes" indent="no" />

<xsl:variable name="nl">
	<xsl:text>&#13;&#10;</xsl:text>
</xsl:variable>
<xsl:variable name="brnl">
	<br /><xsl:copy-of select="$nl" />
</xsl:variable>

<xsl:variable name="HtmlHeader">
	<xsl:text disable-output-escaping="yes"><![CDATA[<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"]]></xsl:text>
		<xsl:copy-of select="$nl" />
	<xsl:text disable-output-escaping="yes"><![CDATA[	"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">]]></xsl:text>
		<xsl:copy-of select="$nl" />
	<xsl:text disable-output-escaping="yes"><![CDATA[<html xmlns="http://www.w3.org/1999/xhtml">]]></xsl:text>
		<xsl:copy-of select="$nl" />
</xsl:variable>

<xsl:variable name="HtmlFooter">
	<xsl:text disable-output-escaping="yes"><![CDATA[</html>]]></xsl:text>
		<xsl:copy-of select="$nl" />
</xsl:variable>

<!-- Design Copyright (c) 2003-2017 Dominik Reichl -->
<xsl:variable name="DocStyle">
<xsl:text disable-output-escaping="yes"><![CDATA[<style type="text/css"><!--
body, p, div, h1, h2, h3, h4, h5, h6, ol, ul, li, td, th, dd, dt, a {
	font-family: Verdana, Arial, sans-serif;
	font-size: 13px;
	font-weight: normal;
	color: #000000;
}

body {
	background-color: #FFFFFF;
}

p {
	margin-left: 0px;
}

h2 {
	font-size: 18px;
	font-weight: bold;
}

table.tablebox {
	background-color: #EEEEEE;
	margin: 0px 0px 0px 0px;
	padding: 0px 0px 0px 0px;
	border-left: 1px solid #AFB5CF;
	border-right: 0px none;
	border-top-width: 0px;
	border-bottom-width: 0px;
	border-collapse: collapse;

	width: 100%;
	table-layout: fixed;
}

table.tablebox tr th {
	background-color: #EEEEEE;
	background-image: -webkit-linear-gradient(top, #C0C0C0, #EEEEEE);
	background-image: -moz-linear-gradient(top, #C0C0C0, #EEEEEE);
	background-image: -ms-linear-gradient(top, #C0C0C0, #EEEEEE);
	background-image: linear-gradient(to bottom, #C0C0C0, #EEEEEE);
	font-weight: bold;
	border-bottom: 1px solid #AFB5CF;
	border-left: 0px none;
	border-right: 1px solid #AFB5CF;
	border-top: 1px solid #AFB5CF;
	padding: 2px 2px 2px 5px;
	empty-cells: show;
	white-space: nowrap;
	text-align: left;
	vertical-align: top;
}

table.tablebox tr td {
	background-color: #F0F0F0;
	font-weight: normal;
	border-bottom: 1px solid #AFB5CF;
	border-left: 0px none;
	border-right: 1px solid #AFB5CF;
	border-top: 0px none;
	padding: 5px 5px 5px 5px;
	empty-cells: show;
	text-align: left;
	vertical-align: top;

	overflow-wrap: break-word;
	word-wrap: break-word;
}

a:visited {
	text-decoration: none;
	color: #0000DD;
}

a:active {
	text-decoration: none;
	color: #6699FF;
}

a:link {
	text-decoration: none;
	color: #0000DD;
}

a:hover {
	text-decoration: underline;
	color: #6699FF;
}
--></style>
]]></xsl:text>
</xsl:variable>

<xsl:template match="/">
	<xsl:copy-of select="$HtmlHeader" />
	<head>
		<meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
		<title>KDBX Common</title>
	</head><xsl:copy-of select="$nl" />
	<body>
		<p>The <em>KDBX_Common.xsl</em> stylesheet is not supposed to be used directly.</p>
	</body><xsl:copy-of select="$nl" />
	<xsl:copy-of select="$HtmlFooter" />
</xsl:template>

</xsl:stylesheet>
