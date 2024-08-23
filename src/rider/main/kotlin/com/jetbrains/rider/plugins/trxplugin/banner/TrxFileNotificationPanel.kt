package com.jetbrains.rider.plugins.trxplugin.banner

import com.intellij.ui.EditorNotificationPanel
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.ui.EditorNotifications
import com.jetbrains.rider.plugins.trxplugin.TrxImportService
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlin.coroutines.CoroutineContext

class TrxFileNotificationPanel(project: Project, file: VirtualFile) : EditorNotificationPanel(Status.Info), CoroutineScope {
    override val coroutineContext: CoroutineContext
        get() = Dispatchers.Main
    init {
        text = "Import this TRX as unit test session"
        createActionLabel("Import") {
            launch {
                val trxImportService = project.getService(TrxImportService::class.java)
                val response = trxImportService.importTrx(file.path)
                if (response == "Failed") {
                    file.putUserData(TrxFileNotificationProvider.KEY_IMPORT_FAILED, true)
                    EditorNotifications.getInstance(project).updateNotifications(file)
                }
            }
        }
    }
}
