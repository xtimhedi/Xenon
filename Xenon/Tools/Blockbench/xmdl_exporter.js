(function() {
    var exportAction = new Action('xmdl_exporter', {
        name: 'Export Xenon Model (Text/OBJ)',
        icon: 'icon-objects',
        category: 'file',
        click: function() {
            if (!Project) {
                Blockbench.showQuickMessage('No active project found!', 2000);
                return;
            }

            let baseName = Project.name || 'model';
            let xenonFormatVersion = "1.0.0";

            // 1. Generate standard OBJ geometry
            let rawObjContent = Codecs.obj.compile({ export_selected: false });

            if (!rawObjContent || rawObjContent.trim() === "") {
                Blockbench.showQuickMessage('❌ Export Failed: No geometry generated.', 4000);
                return;
            }

            // 2. Build the Modified Header
            let customHeader = `# XenonEngine Modified OBJ Format\n`;
            customHeader += `xenon_format_version ${xenonFormatVersion}\n`;
            customHeader += `xenon_mesh_def true\n\n`;

            // Combine header with standard OBJ data
            let finalContent = customHeader + rawObjContent;

            // 3. Generate MTL content
            let rawMtlContent = '# Xenon Material Template\n';
            Texture.all.forEach(tex => {
                let matName = tex.name.split('.')[0]; 
                rawMtlContent += `newmtl ${matName}\nmap_Kd ${tex.name}\n\n`;
            });

            // 4. Export
            if (isApp) {
                Blockbench.export({
                    type: 'Xenon Text Model',
                    extensions: ['xmdl'], // Keeping your custom extension
                    name: baseName + '.xmdl',
                    content: finalContent,
                    custom_writer: function(content, filePath) {
                        const fs = require('fs');
                        const path = require('path');
                        
                        fs.writeFileSync(filePath, content, 'utf-8');
                        
                        let mtlPath = path.join(path.dirname(filePath), path.basename(filePath, '.xmdl') + '.xmtl');
                        fs.writeFileSync(mtlPath, rawMtlContent, 'utf-8');
                        
                        Blockbench.showQuickMessage('Exported Xenon Text Resource!', 3000);
                    }
                });
            } else {
                // Web client export omitted for brevity, but it's just passing 'finalContent' as a string blob
            }
        }
    });

    Plugin.register('xmdl_text_exporter', {
        title: 'XenonEngine Text format exporter',
        author: 'Newertech LLC',
        icon: 'text_snippet',
        description: 'Exports files to a modified OBJ text format for XenonEngine',
        version: '1.3.0',
        variant: 'both',
        onload() { MenuBar.addAction(exportAction, 'file.export'); },
        onunload() { exportAction.delete(); }
    });
})();