package com.jetbrains.rider.plugins.trxplugin.fileType

import com.intellij.openapi.fileTypes.FileTypeConsumer
import com.intellij.openapi.fileTypes.FileTypeFactory

class TrxFileTypeFactory : FileTypeFactory() {
    override fun createFileTypes(consumer: FileTypeConsumer) {
        consumer.consume(TrxFileType.INSTANCE, TrxFileType.INSTANCE.defaultExtension)
    }
}
