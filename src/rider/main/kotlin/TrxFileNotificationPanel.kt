import com.intellij.openapi.editor.colors.ColorKey
import com.intellij.openapi.editor.colors.EditorColors
import com.intellij.ui.EditorNotificationPanel
import com.intellij.openapi.project.Project
import com.intellij.openapi.ui.Messages
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.openapi.wm.impl.status.InfoAndProgressPanel
import com.jetbrains.rider.plugins.trxplugin.ProtocolCaller
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import java.awt.Color
import kotlin.coroutines.CoroutineContext

class TrxFileNotificationPanel(project: Project, file: VirtualFile) : EditorNotificationPanel(Status.Info), CoroutineScope {
    override val coroutineContext: CoroutineContext
        get() = Dispatchers.Main
    init {
        text = "Import this TRX as unit test session"
        createActionLabel("Import") {
            launch {
                val protocolCaller = project.getService(ProtocolCaller::class.java)
                val response = protocolCaller.doCall(file.path)
            }
        }
    }
}
