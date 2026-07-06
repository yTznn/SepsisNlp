import { useState } from 'react';
import axios from 'axios';
import { Search, Loader2, ClipboardList, Activity, ArrowRightCircle } from 'lucide-react';

export default function GarimpoCoorte({ onExportar }) {
    const [searchCid, setSearchCid] = useState('');
    const [foundPatients, setFoundPatients] = useState([]);
    const [loadingPatients, setLoadingPatients] = useState(false);
    const [reportsError, setReportsError] = useState('');

    const handleSearchPatientsByCid = async (e) => {
        e.preventDefault();
        if (!searchCid) return;

        setLoadingPatients(true);
        setReportsError('');
        setFoundPatients([]);

        try {
            const response = await axios.get(`http://localhost:5056/api/Reports/patients-by-cid?cid=${searchCid}`);
            setFoundPatients(response.data);
            if (response.data.length === 0) {
                setReportsError('Nenhum paciente encontrado com este CID na base de evoluções.');
            }
        } catch (err) {
            setReportsError('Erro ao buscar pacientes. Verifique se a API está rodando.');
        } finally {
            setLoadingPatients(false);
        }
    };

    return (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 flex flex-col h-full overflow-hidden animate-in fade-in duration-500 max-w-2xl mx-auto">
            <div className="p-6 border-b border-slate-100 bg-slate-50/50">
                <div className="flex items-center gap-3 mb-2">
                    <div className="bg-blue-100 p-2 rounded-lg">
                        <Activity className="w-5 h-5 text-blue-600" />
                    </div>
                    <h3 className="text-lg font-bold text-slate-800">Triagem de Coorte</h3>
                </div>
                <p className="text-sm text-slate-500">Localize os prontuários da sua amostra pelo CID.</p>
            </div>

            <div className="p-6 flex-1 flex flex-col min-h-[400px]">
                <form onSubmit={handleSearchPatientsByCid} className="flex gap-3 mb-6 shrink-0">
                    <div className="relative flex-1">
                        <Search className="w-5 h-5 text-slate-400 absolute left-3 top-3.5" />
                        <input 
                            type="text" 
                            placeholder="Digite o CID (Ex: J159, A419)" 
                            value={searchCid}
                            onChange={(e) => setSearchCid(e.target.value)}
                            className="w-full border-2 border-slate-200 rounded-xl py-3 pl-10 pr-4 text-base focus:ring-4 focus:ring-blue-100 focus:border-blue-400 focus:outline-none uppercase transition-all" 
                        />
                    </div>
                    <button 
                        type="submit" 
                        disabled={loadingPatients || !searchCid}
                        className="bg-blue-600 hover:bg-blue-700 text-white font-bold px-6 rounded-xl transition-all shadow-md disabled:opacity-50 flex items-center justify-center"
                    >
                        {loadingPatients ? <Loader2 className="w-5 h-5 animate-spin" /> : 'Buscar'}
                    </button>
                </form>

                {reportsError && (
                    <div className="p-3 bg-rose-50 text-rose-600 rounded-lg text-sm font-medium mb-4 shrink-0">
                        {reportsError}
                    </div>
                )}

                <div className="flex-1 overflow-y-auto custom-scrollbar border border-slate-100 rounded-xl bg-slate-50/50 p-2">
                    {foundPatients.length === 0 && !loadingPatients && !reportsError && (
                        <div className="h-full flex flex-col items-center justify-center text-slate-400 space-y-3 p-8">
                            <ClipboardList className="w-12 h-12 opacity-50" />
                            <p className="text-sm font-medium text-center">Os prontuários encontrados aparecerão aqui.</p>
                        </div>
                    )}

                    {foundPatients.length > 0 && (
                        <div className="space-y-2">
                            <p className="text-xs font-bold text-slate-400 uppercase tracking-wider mb-3 px-2">
                                {foundPatients.length} Paciente(s) Encontrado(s)
                            </p>
                            {foundPatients.map((patient, idx) => (
                                <div key={idx} className="bg-white border border-slate-200 p-4 rounded-lg flex items-center justify-between shadow-sm hover:border-blue-300 transition-colors group">
                                    <div>
                                        <p className="font-mono font-bold text-slate-700">{patient.prontuario}</p>
                                        <p className="text-xs text-slate-500 font-medium mt-1">Nasc: {patient.dataNascimento}</p>
                                    </div>
                                    <button 
                                        onClick={() => onExportar(patient.prontuario)}
                                        className="text-xs font-bold bg-blue-50 text-blue-600 hover:bg-blue-600 hover:text-white px-3 py-2 rounded-lg flex items-center gap-2 transition-all opacity-0 group-hover:opacity-100"
                                        title="Enviar para Exportação"
                                    >
                                        Extrair Dados <ArrowRightCircle className="w-4 h-4" />
                                    </button>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}