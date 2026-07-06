import { useState, useRef } from 'react';
import axios from 'axios';
import { UploadCloud, FileText, Loader2, CheckCircle2, AlertCircle, X } from 'lucide-react';

export default function ImportacaoCsv() {
    // Estados para Evolução Assistencial
    const [fileAssistential, setFileAssistential] = useState(null);
    const [loadingAssistential, setLoadingAssistential] = useState(false);
    const [statusAssistential, setStatusAssistential] = useState(null); // { type: 'success' | 'error', msg: '' }
    const assistentialInputRef = useRef(null);

    // Estados para Evolução Clínica (Médica)
    const [fileMedical, setFileMedical] = useState(null);
    const [loadingMedical, setLoadingMedical] = useState(false);
    const [statusMedical, setStatusMedical] = useState(null);
    const medicalInputRef = useRef(null);

    // Manipuladores de Arquivo
    const handleFileChange = (e, type) => {
        const file = e.target.files[0];
        if (!file) return;

        if (type === 'assistential') {
            setFileAssistential(file);
            setStatusAssistential(null);
        } else {
            setFileMedical(file);
            setStatusMedical(null);
        }
    };

    const clearFile = (type) => {
        if (type === 'assistential') {
            setFileAssistential(null);
            assistentialInputRef.current.value = '';
        } else {
            setFileMedical(null);
            medicalInputRef.current.value = '';
        }
    };

    // Lógica de Upload para a API
    const handleUpload = async (type) => {
        const file = type === 'assistential' ? fileAssistential : fileMedical;
        if (!file) return;

        const setLoading = type === 'assistential' ? setLoadingAssistential : setLoadingMedical;
        const setStatus = type === 'assistential' ? setStatusAssistential : setStatusMedical;
        const endpoint = type === 'assistential' 
            ? 'http://localhost:5056/api/Imports/assistential-evolutions' 
            : 'http://localhost:5056/api/Imports/medical-evolutions';

        setLoading(true);
        setStatus(null);

        const formData = new FormData();
        formData.append('file', file);

        try {
            const response = await axios.post(endpoint, formData, {
                headers: { 'Content-Type': 'multipart/form-data' }
            });
            
            setStatus({ 
                type: 'success', 
                msg: `Processamento iniciado! ${response.data.evolutionsSentToQueue || ''} registros enviados para a fila.` 
            });
            
            // Limpa o arquivo após sucesso
            if (type === 'assistential') {
                setFileAssistential(null);
                assistentialInputRef.current.value = '';
            } else {
                setFileMedical(null);
                medicalInputRef.current.value = '';
            }

        } catch (err) {
            console.error(err);
            setStatus({ type: 'error', msg: 'Erro ao enviar o arquivo. Verifique se a API está rodando.' });
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="max-w-5xl mx-auto space-y-8 animate-in fade-in duration-500">
            
            <div className="bg-white p-6 rounded-xl shadow-sm border border-slate-200">
                <h2 className="text-xl font-bold text-slate-800 mb-2 flex items-center gap-2">
                    <UploadCloud className="w-6 h-6 text-blue-600" /> Pipeline de Ingestão de Dados
                </h2>
                <p className="text-sm text-slate-500">
                    Faça o upload dos arquivos CSV gerados pelo sistema hospitalar. Os dados serão enviados diretamente para a fila do RabbitMQ, garantindo processamento assíncrono e anonimização automática (Privacy by Design).
                </p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                
                {/* CARD 1: EVOLUÇÃO ASSISTENCIAL */}
                <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden flex flex-col">
                    <div className="bg-emerald-50 border-b border-emerald-100 p-4">
                        <h3 className="font-bold text-emerald-800">Evoluções Assistenciais</h3>
                        <p className="text-xs text-emerald-600 mt-1">Enfermagem, Fisioterapia, Nutrição, etc.</p>
                    </div>
                    
                    <div className="p-6 flex-1 flex flex-col">
                        <div className="border-2 border-dashed border-slate-300 rounded-xl p-8 text-center bg-slate-50 hover:bg-slate-100 transition-colors relative mb-4">
                            <input 
                                type="file" 
                                accept=".csv"
                                ref={assistentialInputRef}
                                onChange={(e) => handleFileChange(e, 'assistential')}
                                className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
                                disabled={loadingAssistential}
                            />
                            
                            {!fileAssistential ? (
                                <div className="pointer-events-none">
                                    <UploadCloud className="w-10 h-10 text-slate-400 mx-auto mb-3" />
                                    <p className="text-sm font-semibold text-slate-600">Clique ou arraste o CSV aqui</p>
                                    <p className="text-xs text-slate-400 mt-1">Apenas arquivos .csv</p>
                                </div>
                            ) : (
                                <div className="flex flex-col items-center pointer-events-none">
                                    <FileText className="w-10 h-10 text-emerald-500 mx-auto mb-3" />
                                    <p className="text-sm font-bold text-slate-700 truncate max-w-[200px]">{fileAssistential.name}</p>
                                    <p className="text-xs text-slate-500 mt-1">{(fileAssistential.size / 1024 / 1024).toFixed(2)} MB</p>
                                </div>
                            )}

                            {/* Botão de limpar arquivo por cima do input invísivel (precisa de z-index) */}
                            {fileAssistential && !loadingAssistential && (
                                <button 
                                    onClick={(e) => { e.stopPropagation(); clearFile('assistential'); }}
                                    className="absolute top-2 right-2 p-1 bg-rose-100 text-rose-600 rounded-full hover:bg-rose-200 z-10 transition-colors"
                                >
                                    <X className="w-4 h-4" />
                                </button>
                            )}
                        </div>

                        {statusAssistential && (
                            <div className={`p-3 rounded-lg text-sm font-medium flex items-center gap-2 mb-4 ${
                                statusAssistential.type === 'success' ? 'bg-emerald-50 text-emerald-700 border border-emerald-200' : 'bg-rose-50 text-rose-700 border border-rose-200'
                            }`}>
                                {statusAssistential.type === 'success' ? <CheckCircle2 className="w-5 h-5 shrink-0" /> : <AlertCircle className="w-5 h-5 shrink-0" />}
                                {statusAssistential.msg}
                            </div>
                        )}

                        <button 
                            onClick={() => handleUpload('assistential')}
                            disabled={!fileAssistential || loadingAssistential}
                            className="mt-auto w-full bg-emerald-600 hover:bg-emerald-700 text-white font-bold py-3 rounded-lg flex items-center justify-center gap-2 transition-all shadow-md disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            {loadingAssistential ? <Loader2 className="w-5 h-5 animate-spin" /> : <UploadCloud className="w-5 h-5" />}
                            {loadingAssistential ? 'Enfileirando...' : 'Iniciar Importação Assistencial'}
                        </button>
                    </div>
                </div>

                {/* CARD 2: EVOLUÇÃO CLÍNICA */}
                <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden flex flex-col">
                    <div className="bg-purple-50 border-b border-purple-100 p-4">
                        <h3 className="font-bold text-purple-800">Evoluções Clínicas</h3>
                        <p className="text-xs text-purple-600 mt-1">Evoluções Médicas (UTI, Enfermaria, etc.)</p>
                    </div>
                    
                    <div className="p-6 flex-1 flex flex-col">
                        <div className="border-2 border-dashed border-slate-300 rounded-xl p-8 text-center bg-slate-50 hover:bg-slate-100 transition-colors relative mb-4">
                            <input 
                                type="file" 
                                accept=".csv"
                                ref={medicalInputRef}
                                onChange={(e) => handleFileChange(e, 'medical')}
                                className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
                                disabled={loadingMedical}
                            />
                            
                            {!fileMedical ? (
                                <div className="pointer-events-none">
                                    <UploadCloud className="w-10 h-10 text-slate-400 mx-auto mb-3" />
                                    <p className="text-sm font-semibold text-slate-600">Clique ou arraste o CSV aqui</p>
                                    <p className="text-xs text-slate-400 mt-1">Apenas arquivos .csv</p>
                                </div>
                            ) : (
                                <div className="flex flex-col items-center pointer-events-none">
                                    <FileText className="w-10 h-10 text-purple-500 mx-auto mb-3" />
                                    <p className="text-sm font-bold text-slate-700 truncate max-w-[200px]">{fileMedical.name}</p>
                                    <p className="text-xs text-slate-500 mt-1">{(fileMedical.size / 1024 / 1024).toFixed(2)} MB</p>
                                </div>
                            )}

                            {fileMedical && !loadingMedical && (
                                <button 
                                    onClick={(e) => { e.stopPropagation(); clearFile('medical'); }}
                                    className="absolute top-2 right-2 p-1 bg-rose-100 text-rose-600 rounded-full hover:bg-rose-200 z-10 transition-colors"
                                >
                                    <X className="w-4 h-4" />
                                </button>
                            )}
                        </div>

                        {statusMedical && (
                            <div className={`p-3 rounded-lg text-sm font-medium flex items-center gap-2 mb-4 ${
                                statusMedical.type === 'success' ? 'bg-purple-50 text-purple-700 border border-purple-200' : 'bg-rose-50 text-rose-700 border border-rose-200'
                            }`}>
                                {statusMedical.type === 'success' ? <CheckCircle2 className="w-5 h-5 shrink-0" /> : <AlertCircle className="w-5 h-5 shrink-0" />}
                                {statusMedical.msg}
                            </div>
                        )}

                        <button 
                            onClick={() => handleUpload('medical')}
                            disabled={!fileMedical || loadingMedical}
                            className="mt-auto w-full bg-purple-600 hover:bg-purple-700 text-white font-bold py-3 rounded-lg flex items-center justify-center gap-2 transition-all shadow-md disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            {loadingMedical ? <Loader2 className="w-5 h-5 animate-spin" /> : <UploadCloud className="w-5 h-5" />}
                            {loadingMedical ? 'Enfileirando...' : 'Iniciar Importação Médica'}
                        </button>
                    </div>
                </div>

            </div>
        </div>
    );
}