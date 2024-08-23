package com.jetbrains.rider.plugins.trxplugin.action

import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.actionSystem.CommonDataKeys
import com.intellij.openapi.ui.Messages
import com.jetbrains.rider.plugins.trxplugin.TrxImportService
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlin.coroutines.CoroutineContext


class TrxFileProjectViewAction : AnAction(), CoroutineScope {

    override val coroutineContext: CoroutineContext
        get() = Dispatchers.Main

    override fun actionPerformed(event: AnActionEvent) {
        val file = event.getData(CommonDataKeys.VIRTUAL_FILE)
        val project = event.project
        launch {
            val trxImportService = project?.getService(TrxImportService::class.java)
            val response = file?.let { trxImportService?.importTrx(it.path) }
            if (response == "Failed") {
                Messages.showErrorDialog("Failed to import TRX file", "Import TRX")
            }
        }
    }
}
