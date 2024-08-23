package com.jetbrains.rider.plugins.trxplugin.fileType

import com.intellij.ide.highlighter.XmlLikeFileType
import com.intellij.lang.xml.XMLLanguage
import javax.swing.Icon


class TrxFileType private constructor() : XmlLikeFileType(XMLLanguage.INSTANCE) {
    override fun getName(): String {
        return "Trx"
    }

    override fun getDescription(): String {
        return "Trx file"
    }

    override fun getDefaultExtension(): String {
        return "trx"
    }

    override fun getIcon(): Icon {
        return TrxIcon.FILE
    }

    companion object {
        val INSTANCE: TrxFileType = TrxFileType()
    }
}
