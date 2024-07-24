import com.intellij.openapi.fileEditor.FileEditorManager
import com.intellij.openapi.fileEditor.FileEditorManagerListener
import com.intellij.openapi.ui.Messages
import com.intellij.openapi.vfs.VirtualFile
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import com.jetbrains.rider.plugins.simpleplugin.ProtocolCaller
import kotlin.coroutines.CoroutineContext


class TrxFileDropHandler : FileEditorManagerListener, CoroutineScope {
    override val coroutineContext: CoroutineContext
        get() = Dispatchers.Main
    override fun fileOpened(source: FileEditorManager, file: VirtualFile) {
        if (file.extension == "trx") {
            val project = source.project

            launch {
                val protocolCaller = project.getService(ProtocolCaller::class.java)
                val response = protocolCaller.doCall(file.path)
                Messages.showMessageDialog(
                    project,
                    "TRX file opened: ${file.name}",
                    response,
                    Messages.getInformationIcon()
                )
            }
        }
    }

    override fun fileClosed(source: FileEditorManager, file: VirtualFile) {
        if (file.extension == "trx") {
            val project = source.project

            Messages.showMessageDialog(
                project,
                "TRX file closed: ${file.name}",
                "File Close",
                Messages.getInformationIcon()
            )
        }
    }
}
