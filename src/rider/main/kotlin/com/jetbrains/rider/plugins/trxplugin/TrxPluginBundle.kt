package com.jetbrains.rider.plugins.trxplugin

import com.intellij.DynamicBundle
import org.jetbrains.annotations.PropertyKey

object TrxPluginBundle : DynamicBundle("messages.TrxPlugin") {
    fun message(@PropertyKey(resourceBundle = "messages.TrxPlugin") key: String, vararg params: Any): String {
        return getMessage(key, *params)
    }
}
