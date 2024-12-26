package com.jetbrains.rider.plugins.trxplugin.banner

import com.intellij.openapi.project.Project
import com.intellij.openapi.ui.Messages
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.ui.EditorNotificationPanel
import com.intellij.ui.EditorNotifications
import com.jetbrains.rd.util.threading.coroutines.asCoroutineDispatcher
import com.jetbrains.rider.plugins.trxplugin.TrxImportService
import com.jetbrains.rider.plugins.trxplugin.TrxPluginBundle
import com.jetbrains.rider.plugins.trxplugin.coroutineScope
import com.jetbrains.rider.protocol.protocol
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext

class TrxFileNotificationPanel(project: Project, file: VirtualFile) : EditorNotificationPanel(Status.Info) {

    init {
        text = TrxPluginBundle.message("action.TrxFileProjectViewAction.text")
        createActionLabel(TrxPluginBundle.message("import.label")) {
            project.coroutineScope.launch {
                val trxImportService = project.getService(TrxImportService::class.java)
                val response = withContext(project.protocol.scheduler.asCoroutineDispatcher) {
                    trxImportService.importTrx(file.path)
                }
                if (response.result == "Failed") {
                    file.putUserData(TrxFileNotificationProvider.KEY_IMPORT_FAILED, true)
                    EditorNotifications.getInstance(project).updateNotifications(file)
                    Messages.showErrorDialog(
                        TrxPluginBundle.message(response.message),
                        TrxPluginBundle.message("import.message.error.title")
                    )
                }
            }
        }
    }
}
