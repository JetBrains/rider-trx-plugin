package com.jetbrains.rider.plugins.trxplugin.banner

import com.intellij.AbstractBundle
import com.intellij.ui.EditorNotificationPanel
import com.intellij.openapi.project.Project
import com.intellij.openapi.ui.Messages
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.ui.EditorNotifications
import com.jetbrains.rider.plugins.trxplugin.TrxImportService
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import org.jetbrains.annotations.PropertyKey
import kotlin.coroutines.CoroutineContext

object FrontendStrings : AbstractBundle("FrontendStrings") {
    fun message(@PropertyKey(resourceBundle = "FrontendStrings") key: String, vararg params: Any): String {
        return getMessage(key, *params)
    }
}

class TrxFileNotificationPanel(project: Project, file: VirtualFile) : EditorNotificationPanel(Status.Info),
    CoroutineScope {
    override val coroutineContext: CoroutineContext
        get() = Dispatchers.Main

    init {
        text = FrontendStrings.message("import.trx.session")
        createActionLabel(FrontendStrings.message("import.label")) {
            launch {
                val trxImportService = project.getService(TrxImportService::class.java)
                val response = trxImportService.importTrx(file.path)
                if (response == "Failed") {
                    file.putUserData(TrxFileNotificationProvider.KEY_IMPORT_FAILED, true)
                    EditorNotifications.getInstance(project).updateNotifications(file)
                    Messages.showErrorDialog(
                        "Failed to import TRX file",
                        FrontendStrings.message("import.message.title")
                    )
                }
            }
        }
    }
}
