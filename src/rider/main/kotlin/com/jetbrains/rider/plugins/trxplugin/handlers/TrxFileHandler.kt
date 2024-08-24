package com.jetbrains.rider.plugins.trxplugin.handlers

import com.intellij.openapi.fileEditor.FileEditorManager
import com.intellij.openapi.fileEditor.FileEditorManagerListener
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.ui.EditorNotifications

class TrxFileHandler : FileEditorManagerListener {
    override fun fileOpened(source: FileEditorManager, file: VirtualFile) {
        if (file.extension == "trx") {
            val project = source.project
            EditorNotifications.getInstance(project).updateNotifications(file)
        }
    }
}
