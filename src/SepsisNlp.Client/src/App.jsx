import { useState } from 'react';
import { LayoutDashboard, UploadCloud, Search, FileJson, ShieldAlert, User } from 'lucide-react';

// Importação dos nossos novos componentes sêniors!
import DashboardClinico from './components/DashboardClinico';
import ImportacaoCsv from './components/ImportacaoCsv';
import GarimpoCoorte from './components/GarimpoCoorte';
import ExportarDataset from './components/ExportarDataset';
import AuditoriaLgpd from './components/AuditoriaLgpd';

export default function App() {
    const [activeTab, setActiveTab] = useState('dashboard');
    
    // Estado global para passar o prontuário do Garimpo direto para a tela de Exportação
    const [exportPseudonym, setExportPseudonym] = useState('');

    const goToExport = (pseudonym) => {
        setExportPseudonym(pseudonym);
        setActiveTab('exportar');
    };

    return (
        <div className="min-h-screen bg-slate-50 flex font-sans text-slate-800">
            {/* NAVBAR LATERAL */}
            <aside className="w-64 bg-slate-900 text-slate-300 flex flex-col shadow-2xl z-10 shrink-0">
                <div className="p-6 border-b border-slate-800">
                    <h1 className="text-xl font-bold tracking-wider text-slate-100 flex items-center gap-2">
                        SEPSIS<span className="text-emerald-400">NLP</span>
                    </h1>
                    <p className="text-xs text-slate-500 mt-1">Research Environment</p>
                </div>

                <div className="p-4 flex-1 space-y-2 overflow-y-auto custom-scrollbar">
                    <button onClick={() => setActiveTab('dashboard')} className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-semibold transition-all ${activeTab === 'dashboard' ? 'bg-blue-600 text-white shadow-md' : 'hover:bg-slate-800 hover:text-white'}`}>
                        <LayoutDashboard className="w-5 h-5" /> Dashboard Clínico
                    </button>

                    <button onClick={() => setActiveTab('importacao')} className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-semibold transition-all ${activeTab === 'importacao' ? 'bg-blue-600 text-white shadow-md' : 'hover:bg-slate-800 hover:text-white'}`}>
                        <UploadCloud className="w-5 h-5" /> Importação de Dados
                    </button>

                    <div className="my-4 border-t border-slate-800"></div>

                    <button onClick={() => setActiveTab('garimpo')} className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-semibold transition-all ${activeTab === 'garimpo' ? 'bg-emerald-600 text-white shadow-md' : 'hover:bg-slate-800 hover:text-white'}`}>
                        <Search className="w-5 h-5" /> Triagem de Coorte
                    </button>

                    <button onClick={() => setActiveTab('exportar')} className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-semibold transition-all ${activeTab === 'exportar' ? 'bg-emerald-600 text-white shadow-md' : 'hover:bg-slate-800 hover:text-white'}`}>
                        <FileJson className="w-5 h-5" /> Exportar Dataset Paciente
                    </button>
                    
                    <div className="my-4 border-t border-slate-800"></div>

                    <button onClick={() => setActiveTab('auditoria')} className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-semibold transition-all ${activeTab === 'auditoria' ? 'bg-rose-600 text-white shadow-md' : 'hover:bg-slate-800 hover:text-white'}`}>
                        <ShieldAlert className="w-5 h-5" /> Auditoria & LGPD
                    </button>
                </div>

                <div className="p-4 border-t border-slate-800 shrink-0">
                    <div className="flex items-center gap-3">
                        <div className="bg-slate-700 p-2 rounded-full"><User className="w-5 h-5 text-emerald-400" /></div>
                        <div>
                            <p className="text-sm font-bold text-slate-200">Elielton de Souza</p>
                            <p className="text-xs text-emerald-400 font-medium">Pesquisador</p>
                        </div>
                    </div>
                </div>
            </aside>

            {/* ÁREA PRINCIPAL */}
            <main className="flex-1 flex flex-col h-screen overflow-hidden">
                <header className="bg-white border-b border-slate-200 p-6 flex justify-between items-center shadow-sm shrink-0">
                    <h2 className="text-2xl font-bold text-slate-800">
                        {activeTab === 'dashboard' && 'Evoluções Multidisciplinares'}
                        {activeTab === 'importacao' && 'Pipeline de Importação (RabbitMQ)'}
                        {activeTab === 'garimpo' && 'Garimpo Analítico por CID'}
                        {activeTab === 'exportar' && 'Extração do Dataset (JSON)'}
                        {activeTab === 'auditoria' && 'Cofre de Desanonimização'}
                    </h2>
                </header>

                <div className="p-8 flex-1 overflow-y-auto">
                    {activeTab === 'dashboard' && <DashboardClinico />}
                    {activeTab === 'importacao' && <ImportacaoCsv />}
                    {activeTab === 'garimpo' && <GarimpoCoorte onExportar={goToExport} />}
                    {activeTab === 'exportar' && <ExportarDataset exportPseudonym={exportPseudonym} setExportPseudonym={setExportPseudonym} />}
                    {activeTab === 'auditoria' && <AuditoriaLgpd />}
                </div>
            </main>
        </div>
    );
}