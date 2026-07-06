import { useState } from 'react';
import axios from 'axios';
import { FileJson, Loader2, Download, CheckCircle2 } from 'lucide-react';

export default function ExportarDataset({ exportPseudonym, setExportPseudonym }) {
    const [isExporting, setIsExporting] = useState(false);
    const [exportSuccess, setExportSuccess] = useState(false);

    const handleExportPatientDataset = async (e) => {
        e.preventDefault();
        if (!exportPseudonym) return;

        setIsExporting(true);
        setExportSuccess(false);

        try {
            const response = await axios.get(`http://localhost:5056/api/Reports/export/patient/${exportPseudonym}`);
            
            // Cria e baixa o JSON formatado
            const jsonString = JSON.stringify(response.data, null, 2);
            const blob = new Blob([jsonString], { type: 'application/json' });
            
            const href = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = href;
            link.download = `dataset_${exportPseudonym}_${new Date().getTime()}.json`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            URL.revokeObjectURL(href);

            setExportSuccess(true);
            setTimeout(() => setExportSuccess(false), 3000); // Tira o aviso de sucesso após 3s
        } catch (err) {
            console.error(err);
            alert("Erro ao exportar. Verifique se o prontuário está correto e se possui evoluções na base.");
        } finally {
            setIsExporting(false);
        }
    };

    return (
        <div className="bg-slate-900 rounded-xl shadow-xl border border-slate-800 flex flex-col h-full overflow-hidden relative animate-in fade-in duration-500 max-w-2xl mx-auto">
            {/* Detalhe visual de fundo */}
            <FileJson className="absolute -bottom-10 -right-10 w-64 h-64 text-slate-800 opacity-50 pointer-events-none" />
            
            <div className="p-6 border-b border-slate-800 bg-slate-800/50 relative z-10">
                <div className="flex items-center gap-3 mb-2">
                    <div className="bg-emerald-500/20 p-2 rounded-lg">
                        <FileJson className="w-5 h-5 text-emerald-400" />
                    </div>
                    <h3 className="text-lg font-bold text-white">Extração do Dataset (JSON)</h3>
                </div>
                <p className="text-sm text-slate-400">Gere a linha do tempo clínica completa para o LLM.</p>
            </div>

            <div className="p-8 flex-1 flex flex-col justify-center relative z-10 min-h-[400px]">
                
                <form onSubmit={handleExportPatientDataset} className="space-y-6 max-w-md w-full mx-auto">
                    <div>
                        <label className="block text-xs font-bold text-slate-400 uppercase tracking-wider mb-2 text-center">
                            Prontuário do Paciente Alvo
                        </label>
                        <input
                            type="text"
                            value={exportPseudonym}
                            onChange={(e) => setExportPseudonym(e.target.value)}
                            className="w-full bg-slate-800 border-2 border-slate-700 rounded-xl p-4 text-center text-xl font-mono text-emerald-400 focus:ring-4 focus:ring-emerald-500/20 focus:border-emerald-500 focus:outline-none uppercase transition-all shadow-inner"
                            placeholder="Ex: PAC-A1B2C3D4"
                        />
                    </div>

                    <button 
                        type="submit" 
                        disabled={isExporting || !exportPseudonym}
                        className="w-full bg-emerald-600 hover:bg-emerald-500 text-white font-bold py-4 rounded-xl transition-all shadow-[0_0_20px_rgba(5,150,105,0.3)] hover:shadow-[0_0_30px_rgba(5,150,105,0.5)] flex justify-center items-center gap-3 disabled:opacity-50 disabled:shadow-none text-base"
                    >
                        {isExporting ? <Loader2 className="w-6 h-6 animate-spin" /> : <Download className="w-6 h-6" />}
                        {isExporting ? 'Processando Histórico...' : 'Gerar e Baixar JSON (Suco)'}
                    </button>

                    {exportSuccess && (
                        <div className="bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 px-4 py-3 rounded-xl flex items-center justify-center gap-2 text-sm font-bold animate-in fade-in slide-in-from-bottom-2">
                            <CheckCircle2 className="w-5 h-5" /> Download Concluído!
                        </div>
                    )}
                </form>

                <div className="mt-auto pt-12">
                    <div className="bg-slate-800/50 border border-slate-700/50 p-4 rounded-xl">
                        <p className="text-xs text-slate-400 leading-relaxed text-center">
                            A extração ignora restrições de CID e consolida <strong>absolutamente todas as evoluções</strong> do paciente (Assistencial e Médica) registradas na base, agrupadas cronologicamente por atendimento anonimizado.
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
}