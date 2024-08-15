import com.intellij.ui.EditorNotificationPanel
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
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
            }
        }
    }
}
