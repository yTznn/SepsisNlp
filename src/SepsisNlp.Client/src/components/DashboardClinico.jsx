import { useState, useEffect } from 'react';
import axios from 'axios';
import { Search, Loader2, ChevronLeft, ChevronRight, Eye, EyeOff, X, ShieldAlert, User } from 'lucide-react';

export default function DashboardClinico() {
    const [evolutions, setEvolutions] = useState([]);
    const [loadingTable, setLoadingTable] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [currentPage, setCurrentPage] = useState(1);
    const itemsPerPage = 10;

    const [selectedEvolution, setSelectedEvolution] = useState(null);
    const [isTextRevealed, setIsTextRevealed] = useState(false);

    useEffect(() => {
        fetchEvolutions();
    }, []);

    useEffect(() => {
        setCurrentPage(1);
    }, [searchTerm]);

    const fetchEvolutions = async () => {
        try {
            const response = await axios.get('http://localhost:5056/api/Evolutions');
            setEvolutions(response.data);
        } catch (err) {
            console.error('Erro ao buscar evoluções:', err);
        } finally {
            setLoadingTable(false);
        }
    };

    const openEvolutionModal = (evol) => {
        setSelectedEvolution(evol);
        setIsTextRevealed(false);
    };

    const closeModal = () => {
        setSelectedEvolution(null);
        setIsTextRevealed(false);
    };

    const filteredEvolutions = evolutions.filter(evol => 
        evol.professionalRole.toLowerCase().includes(searchTerm.toLowerCase()) ||
        evol.type.toLowerCase().includes(searchTerm.toLowerCase()) ||
        evol.originalEvolutionCode.toLowerCase().includes(searchTerm.toLowerCase())
    );

    const totalPages = Math.ceil(filteredEvolutions.length / itemsPerPage) || 1;
    const currentItems = filteredEvolutions.slice((currentPage - 1) * itemsPerPage, currentPage * itemsPerPage);

    return (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 flex flex-col animate-in fade-in duration-500">
            <div className="p-6 border-b border-slate-100 flex justify-between items-center bg-slate-50/50">
                <div className="relative w-96">
                    <Search className="w-5 h-5 text-slate-400 absolute left-3 top-2.5" />
                    <input 
                        type="text" 
                        placeholder="Pesquisar evolução na tela..." 
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        className="w-full border border-slate-300 rounded-lg py-2.5 pl-10 pr-4 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none transition-all shadow-sm" 
                    />
                </div>
                <span className="bg-blue-50 text-blue-700 border border-blue-200 text-xs font-bold px-4 py-2 rounded-full shadow-sm flex items-center gap-2">
                    <div className="w-2 h-2 rounded-full bg-blue-500 animate-pulse"></div>
                    Base Sincronizada
                </span>
            </div>

            <div className="overflow-x-auto">
                <table className="w-full text-left border-collapse">
                    <thead>
                        <tr className="bg-slate-50 text-slate-500 text-xs uppercase tracking-wider border-b border-slate-200">
                            <th className="p-4 font-semibold w-32">Código</th>
                            <th className="p-4 font-semibold w-48">Data/Hora</th>
                            <th className="p-4 font-semibold w-32">Tipo</th>
                            <th className="p-4 font-semibold">Profissional</th>
                            <th className="p-4 font-semibold text-center w-24">Ações</th>
                        </tr>
                    </thead>
                    <tbody className="text-sm divide-y divide-slate-100">
                        {loadingTable ? (
                            <tr>
                                <td colSpan="5" className="p-16 text-center text-slate-500">
                                    <Loader2 className="w-8 h-8 animate-spin mx-auto mb-3 text-blue-500" />
                                    Sincronizando evoluções...
                                </td>
                            </tr>
                        ) : currentItems.length === 0 ? (
                            <tr>
                                <td colSpan="5" className="p-16 text-center text-slate-500 bg-slate-50/50">Nenhum registro encontrado nesta página.</td>
                            </tr>
                        ) : (
                            currentItems.map((evol, index) => (
                                <tr key={index} className="hover:bg-blue-50/30 transition-colors">
                                    <td className="p-4 text-slate-700 font-bold text-xs">{evol.originalEvolutionCode}</td>
                                    <td className="p-4 text-slate-500 font-medium whitespace-nowrap">{evol.dataHora}</td>
                                    <td className="p-4">
                                        <span className={`px-3 py-1 rounded-md text-[11px] font-bold uppercase tracking-wider shadow-sm border ${
                                            evol.type === 'Assistencial' ? 'bg-emerald-50 text-emerald-700 border-emerald-200' : 'bg-purple-50 text-purple-700 border-purple-200'
                                        }`}>
                                            {evol.type}
                                        </span>
                                    </td>
                                    <td className="p-4 text-slate-700 font-medium truncate max-w-[200px]">{evol.professionalRole}</td>
                                    <td className="p-4 text-center">
                                        <button 
                                            onClick={() => openEvolutionModal(evol)}
                                            className="px-3 py-1.5 bg-white border border-slate-200 text-blue-600 hover:bg-blue-50 hover:border-blue-300 rounded-lg transition-all flex items-center gap-2 mx-auto shadow-sm font-semibold text-xs"
                                        >
                                            <Eye className="w-4 h-4" /> Ler
                                        </button>
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            {!loadingTable && filteredEvolutions.length > 0 && (
                <div className="p-4 border-t border-slate-100 bg-slate-50 flex items-center justify-between rounded-b-xl">
                    <span className="text-xs text-slate-500 font-medium">
                        Mostrando <span className="text-slate-800 font-bold">{currentItems.length}</span> de <span className="text-slate-800 font-bold">{filteredEvolutions.length}</span>
                    </span>
                    <div className="flex gap-2 items-center">
                        <button onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))} disabled={currentPage === 1} className="p-1.5 rounded-md border border-slate-300 bg-white hover:bg-slate-100 disabled:opacity-50">
                            <ChevronLeft className="w-4 h-4" />
                        </button>
                        <span className="px-4 py-1.5 text-xs font-bold text-slate-700 bg-white border border-slate-300 rounded-md">
                            Pág {currentPage} de {totalPages}
                        </span>
                        <button onClick={() => setCurrentPage(prev => Math.min(prev + 1, totalPages))} disabled={currentPage === totalPages} className="p-1.5 rounded-md border border-slate-300 bg-white hover:bg-slate-100 disabled:opacity-50">
                            <ChevronRight className="w-4 h-4" />
                        </button>
                    </div>
                </div>
            )}

            {/* MODAL BLUR */}
            {selectedEvolution && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/70 backdrop-blur-sm animate-in fade-in duration-200">
                    <div className="bg-white rounded-2xl shadow-2xl w-full max-w-4xl overflow-hidden flex flex-col max-h-[90vh] ring-1 ring-white/20">
                        <div className="bg-slate-800 p-6 border-b border-slate-700 flex justify-between items-start shrink-0">
                            <div>
                                <div className="flex items-center gap-3 mb-1.5">
                                    <h3 className="text-2xl font-black text-white font-mono">{selectedEvolution.originalEvolutionCode}</h3>
                                    <span className={`px-2.5 py-1 rounded text-[10px] font-bold uppercase tracking-wider ${
                                        selectedEvolution.type === 'Assistencial' ? 'bg-emerald-500/20 text-emerald-300 border border-emerald-500/30' : 'bg-purple-500/20 text-purple-300 border border-purple-500/30'
                                    }`}>
                                        {selectedEvolution.type}
                                    </span>
                                </div>
                                <p className="text-sm font-medium text-slate-400 flex items-center gap-2">
                                    <User className="w-4 h-4" /> {selectedEvolution.professionalRole} <span className="text-slate-600">•</span> {selectedEvolution.dataHora}
                                </p>
                            </div>
                            <button onClick={closeModal} className="text-slate-400 hover:text-white transition-colors p-2 hover:bg-slate-700 rounded-xl">
                                <X className="w-5 h-5" />
                            </button>
                        </div>

                        <div className="p-8 overflow-y-auto flex-1 relative bg-slate-50 min-h-[400px]">
                            <div className={`transition-all duration-700 h-full ${!isTextRevealed ? 'blur-md select-none opacity-30 grayscale' : 'blur-none opacity-100 grayscale-0'}`}>
                                <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm min-h-full">
                                    <p className="text-slate-700 leading-relaxed whitespace-pre-wrap font-medium text-[15px]">
                                        {selectedEvolution.rawText || "Nenhum texto clínico registrado."}
                                    </p>
                                </div>
                            </div>
                            {!isTextRevealed && (
                                <div className="absolute inset-0 flex flex-col items-center justify-center bg-slate-50/50">
                                    <div className="bg-white p-8 rounded-2xl shadow-xl border border-rose-100 text-center max-w-sm ring-1 ring-slate-900/5">
                                        <div className="bg-rose-50 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-4 border-8 border-white shadow-sm">
                                            <ShieldAlert className="w-8 h-8 text-rose-500" />
                                        </div>
                                        <h4 className="font-bold text-slate-800 text-lg mb-2">Conteúdo Protegido</h4>
                                        <p className="text-sm text-slate-500 mb-6 font-medium leading-relaxed">
                                            A leitura deste material sensível é registrada nos logs de auditoria.
                                        </p>
                                        <button 
                                            onClick={() => setIsTextRevealed(true)}
                                            className="w-full bg-rose-600 hover:bg-rose-700 text-white font-bold py-3.5 rounded-xl flex items-center justify-center gap-2 transition-all shadow-md"
                                        >
                                            <Eye className="w-5 h-5" /> Confirmar e Revelar Texto
                                        </button>
                                    </div>
                                </div>
                            )}
                        </div>
                        
                        {isTextRevealed && (
                            <div className="p-4 bg-slate-100 border-t border-slate-200 flex justify-end shrink-0">
                                <button 
                                    onClick={() => setIsTextRevealed(false)}
                                    className="flex items-center gap-2 text-sm font-bold text-slate-500 hover:text-slate-800 hover:bg-slate-200 px-5 py-2.5 rounded-lg transition-colors"
                                >
                                    <EyeOff className="w-4 h-4" /> Restaurar Proteção
                                </button>
                            </div>
                        )}
                    </div>
                </div>
            )}
        </div>
    );
}