package com.jetbrains.rider.plugins.trxplugin.action

import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.fileChooser.FileChooser
import com.intellij.openapi.fileChooser.FileChooserDescriptor
import com.intellij.openapi.ui.Messages
import com.intellij.openapi.vfs.VirtualFile
import com.jetbrains.rider.plugins.trxplugin.TrxImportService
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlin.coroutines.CoroutineContext

class TrxFileOpenAction : AnAction(), CoroutineScope {
    override val coroutineContext: CoroutineContext
        get() = Dispatchers.Main
    override fun actionPerformed(e: AnActionEvent) {
        val project = e.project ?: return
        val descriptor = FileChooserDescriptor(true, false, false, false, false, false)
            .withFileFilter { it.extension == "trx" }
        val file: VirtualFile? = FileChooser.chooseFile(descriptor, project, null)
        if (file != null) {
            launch {
                val trxImportService = project.getService(TrxImportService::class.java)
                val response = trxImportService.importTrx(file.path)
                if (response.result == "Failed") {
                    Messages.showErrorDialog(
                        FrontendStrings.message(response.message),
                        FrontendStrings.message("import.message.error.title")
                    )
                }
            }
        }

    }
}
