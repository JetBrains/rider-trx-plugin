package com.jetbrains.rider.plugins.trxplugin.handlers

import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.editor.FileDropEvent
import com.intellij.openapi.editor.FileDropHandler
import com.intellij.openapi.fileEditor.FileEditorManager
import com.intellij.openapi.ui.Messages
import com.intellij.openapi.vfs.LocalFileSystem
import com.jetbrains.rider.plugins.trxplugin.TrxImportService
import com.jetbrains.rider.plugins.trxplugin.TrxPluginBundle
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch

class TrxFileDropHandler : FileDropHandler {

    override suspend fun handleDrop(e: FileDropEvent): Boolean {
        for (file in e.files) {
            if (file.extension.equals("trx", ignoreCase = true)) {
                val trxImportService = e.project.getService(TrxImportService::class.java)
                ApplicationManager.getApplication().invokeAndWait {
                    CoroutineScope(Dispatchers.Main).launch {
                        val response = trxImportService.importTrx(file.path)
                        if (response.result == "Failed") {
                            Messages.showErrorDialog(
                                TrxPluginBundle.message(response.message),
                                TrxPluginBundle.message("import.message.error.title")
                            )
                        }
                    }
                }
                continue
            }
            val fileEditorManager = FileEditorManager.getInstance(e.project)

            val virtualFile = LocalFileSystem.getInstance().refreshAndFindFileByIoFile(file)

            ApplicationManager.getApplication().invokeLater {
                virtualFile?.let { fileEditorManager.openFile(it, true) }
            }
        }
        return true
    }
}
