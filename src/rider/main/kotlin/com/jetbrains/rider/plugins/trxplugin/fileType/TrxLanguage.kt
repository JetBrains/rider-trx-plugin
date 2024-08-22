package com.jetbrains.rider.plugins.trxplugin.fileType

import com.intellij.lang.Language

class TrxLanguage private constructor() : Language("Trx") {
    companion object {
        val INSTANCE: TrxLanguage = TrxLanguage()
    }
}
