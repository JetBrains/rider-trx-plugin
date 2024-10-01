package com.jetbrains.rider.plugins.trxplugin.banner

import com.intellij.openapi.fileEditor.FileEditor
import com.intellij.openapi.project.DumbAware
import com.intellij.openapi.project.Project
import com.intellij.openapi.util.Key
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.ui.EditorNotificationProvider
import java.util.function.Function
import javax.swing.JComponent

class TrxFileNotificationProvider : EditorNotificationProvider, DumbAware {

    override fun collectNotificationData(
        project: Project,
        file: VirtualFile
    ): Function<in FileEditor, out JComponent?>? = Function { editor ->
        val importFailed = file.getUserData(KEY_IMPORT_FAILED) == true

        if (file.extension == "trx" && !importFailed) {
            TrxFileNotificationPanel(project, file)
        } else {
            null
        }
    }

    companion object {
        val KEY_IMPORT_FAILED = Key.create<Boolean>("trx.file.importFailed")
    }
}
