<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" indent="no"/>
  <xsl:template match="/">
    <xsl:copy>
      <xsl:apply-templates/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="*">
    <xsl:call-template name="namespace-remover">
      <xsl:with-param name="fullname" select="name()"/>
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="namespace-remover">
    <xsl:param name="fullname"/>
    <xsl:choose>
      <xsl:when test="contains($fullname,':')">
        <xsl:element name="{concat(substring-before($fullname, ':'), '-', substring-after($fullname, ':'))}">
          <xsl:apply-templates select="node()"/>
        </xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="{$fullname}">
          <xsl:apply-templates select="node()"/>
        </xsl:element>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>