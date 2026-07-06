import { useState } from 'react';
import axios from 'axios';
import { ShieldAlert, Loader2, LockKeyhole } from 'lucide-react';

export default function AuditoriaLgpd() {
    const [pseudonym, setPseudonym] = useState('');
    const [patientData, setPatientData] = useState(null);
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const handleDesanonimizar = async (e) => {
        e.preventDefault();
        if (!pseudonym) return;

        setLoading(true);
        setError('');
        setPatientData(null);

        try {
            const response = await axios.get(`http://localhost:5056/api/Audit/rollback/patient/${pseudonym}`);
            setPatientData(response.data);
        } catch (err) {
            setError('Pseudônimo não encontrado no cofre de segurança.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="max-w-2xl mx-auto animate-in fade-in duration-500">
            <div className="bg-white p-8 rounded-xl shadow-sm border border-slate-200 relative overflow-hidden">
                <ShieldAlert className="absolute -bottom-10 -right-10 w-48 h-48 text-rose-50 opacity-50 pointer-events-none" />
                
                <div className="flex items-center gap-3 mb-4 text-rose-600 relative z-10">
                    <div className="bg-rose-100 p-3 rounded-xl">
                        <ShieldAlert className="w-6 h-6 text-rose-600" />
                    </div>
                    <h2 className="text-xl font-bold">Cofre de Desanonimização</h2>
                </div>
                <p className="text-slate-500 mb-8 leading-relaxed font-medium relative z-10 text-sm">
                    Este módulo possui rastreabilidade de auditoria. O uso indevido da desanonimização de dados clínicos de pacientes infringe as diretrizes estabelecidas pela LGPD (Lei Geral de Proteção de Dados Pessoais) e os protocolos do Comitê de Ética.
                </p>

                <form onSubmit={handleDesanonimizar} className="space-y-6 relative z-10">
                    <div>
                        <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Chave Criptográfica (Pseudônimo)</label>
                        <input
                            type="text"
                            value={pseudonym}
                            onChange={(e) => setPseudonym(e.target.value)}
                            className="w-full border-2 border-slate-200 rounded-xl p-4 text-lg font-mono focus:ring-4 focus:ring-rose-100 focus:border-rose-400 focus:outline-none uppercase transition-all shadow-inner bg-slate-50"
                            placeholder="Ex: PAC-A1B2C3D4..."
                        />
                    </div>
                    <button type="submit" disabled={loading} className="w-full bg-slate-800 hover:bg-slate-900 text-white font-bold py-4 rounded-xl transition-all shadow-md flex justify-center items-center gap-3 disabled:opacity-50 text-base">
                        {loading ? <Loader2 className="w-5 h-5 animate-spin" /> : <LockKeyhole className="w-5 h-5" />}
                        {loading ? 'Consultando Camada de Segurança...' : 'Solicitar Quebra de Sigilo e Revelar Dados'}
                    </button>
                </form>

                {error && (
                    <div className="mt-6 p-4 bg-rose-50 text-rose-700 rounded-xl font-semibold border border-rose-200 flex gap-3 items-center shadow-sm relative z-10">
                        <ShieldAlert className="w-5 h-5 shrink-0" /> {error}
                    </div>
                )}

                {patientData && (
                    <div className="mt-8 p-1 bg-gradient-to-r from-emerald-400 to-teal-500 rounded-2xl animate-in fade-in slide-in-from-bottom-4 duration-500 shadow-xl relative z-10">
                        <div className="bg-white rounded-xl p-6">
                            <h3 className="text-sm font-bold text-emerald-600 uppercase mb-5 flex items-center gap-2">
                                <div className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></div>
                                Sigilo Quebrado com Sucesso
                            </h3>
                            
                            <div className="space-y-4">
                                <div className="bg-slate-50 p-4 rounded-lg border border-slate-200 flex justify-between items-center">
                                    <span className="font-bold text-slate-400 uppercase text-[10px] tracking-wider">Prontuário Hospitalar (Origem)</span>
                                    <span className="text-slate-700 font-mono font-bold text-lg">{patientData.realMedicalRecord}</span>
                                </div>
                                <div className="bg-slate-50 p-4 rounded-lg border border-slate-200">
                                    <span className="font-bold text-slate-400 uppercase text-[10px] tracking-wider block mb-1">Identidade do Paciente</span>
                                    <span className="text-slate-800 font-black text-xl">{patientData.realName}</span>
                                </div>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}