package com.jetbrains.rider.plugins.trxplugin.action

import com.intellij.openapi.actionSystem.ActionUpdateThread
import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.actionSystem.CommonDataKeys
import com.intellij.openapi.ui.Messages
import com.jetbrains.rider.plugins.trxplugin.TrxImportService
import com.jetbrains.rider.plugins.trxplugin.TrxPluginBundle
import com.jetbrains.rider.plugins.trxplugin.coroutineScope
import kotlinx.coroutines.launch

class TrxFileProjectViewAction : AnAction() {

    override fun actionPerformed(event: AnActionEvent) {
        val file = event.getData(CommonDataKeys.VIRTUAL_FILE) ?: return
        val project = event.project ?: return
        project.coroutineScope.launch {
            val trxImportService = TrxImportService.getInstance(project)
            val response = trxImportService.importTrx(file.path)
            if (response.result == "Failed") {
                Messages.showErrorDialog(
                    TrxPluginBundle.message(response.message),
                    TrxPluginBundle.message("import.message.error.title")
                )
            }
        }
    }

    override fun update(e: AnActionEvent) {
        val file = e.getData(CommonDataKeys.VIRTUAL_FILE)
        e.presentation.isEnabledAndVisible = file != null && file.extension.equals("trx", ignoreCase = true)
    }

    override fun getActionUpdateThread() = ActionUpdateThread.BGT
}
