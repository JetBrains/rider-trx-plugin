package com.jetbrains.rider.plugins.trxplugin.fileType

import com.intellij.openapi.fileTypes.FileType
import com.intellij.openapi.vfs.VirtualFile
import javax.swing.Icon

class TrxFileType : FileType {
    override fun getName(): String {
        return "TRX"
    }

    override fun getDescription(): String {
        return "TRX file"
    }

    override fun getDefaultExtension(): String {
        return "trx"
    }

    override fun getIcon(): Icon? {
        return null
    }

    override fun isBinary(): Boolean {
        return false
    }

    override fun isReadOnly(): Boolean {
        return false
    }

    override fun getCharset(file: VirtualFile, content: ByteArray): String? {
        return null
    }

    companion object {
        val INSTANCE: FileType = TrxFileType()
    }
}
