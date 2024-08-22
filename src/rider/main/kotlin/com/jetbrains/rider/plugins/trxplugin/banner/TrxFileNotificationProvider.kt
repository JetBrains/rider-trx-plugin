package com.jetbrains.rider.plugins.trxplugin.banner

import com.intellij.openapi.fileEditor.FileEditor
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.ui.EditorNotificationPanel
import com.intellij.openapi.util.Key
import com.intellij.ui.EditorNotifications

class TrxFileNotificationProvider : EditorNotifications.Provider<EditorNotificationPanel>() {

    override fun getKey(): Key<EditorNotificationPanel> {
        return KEY
    }

    override fun createNotificationPanel(file: VirtualFile, fileEditor: FileEditor, project: Project): EditorNotificationPanel? {
        return if (file.extension == "trx") {
            TrxFileNotificationPanel(project, file)
        } else {
            null
        }
    }

    companion object {
        private val KEY = Key.create<EditorNotificationPanel>("trx.file.notification")
    }
}
