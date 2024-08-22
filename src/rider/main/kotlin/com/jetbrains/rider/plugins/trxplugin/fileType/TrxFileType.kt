package com.jetbrains.rider.plugins.trxplugin.fileType

import com.intellij.openapi.fileTypes.LanguageFileType
import javax.swing.Icon


class TrxFileType private constructor() : LanguageFileType(TrxLanguage.INSTANCE) {
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
        return null!!
    }

    companion object {
        val INSTANCE: TrxFileType = TrxFileType()
    }
}
