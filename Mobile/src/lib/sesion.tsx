import * as SecureStore from 'expo-secure-store';
import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { api, configurarSesion, type Sesion } from './api';

const CLAVE = 'mercadex.sesion';

type ContextoSesion = {
  cargando: boolean;
  sesion: Sesion | null;
  ingresar: (documento: string, contrasena: string) => Promise<void>;
  activar: (documento: string, codigo: string, contrasena: string) => Promise<void>;
  renovar: (nueva: Sesion) => Promise<void>;
  salir: () => Promise<void>;
};

const Contexto = createContext<ContextoSesion | null>(null);

export function ProveedorSesion({ children }: { children: ReactNode }) {
  const [cargando, setCargando] = useState(true);
  const [sesion, setSesion] = useState<Sesion | null>(null);

  const salir = useCallback(async () => {
    configurarSesion(null);
    setSesion(null);
    await SecureStore.deleteItemAsync(CLAVE).catch(() => undefined);
  }, []);

  const guardar = useCallback(
    async (nueva: Sesion) => {
      configurarSesion(nueva.token, () => void salir());
      setSesion(nueva);
      await SecureStore.setItemAsync(CLAVE, JSON.stringify(nueva)).catch(() => undefined);
    },
    [salir],
  );

  // Recuperar la sesión guardada al abrir la app.
  useEffect(() => {
    (async () => {
      try {
        const guardada = await SecureStore.getItemAsync(CLAVE);
        if (guardada) {
          const s = JSON.parse(guardada) as Sesion;
          if (new Date(s.expira).getTime() > Date.now()) {
            configurarSesion(s.token, () => void salir());
            setSesion(s);
          } else {
            await SecureStore.deleteItemAsync(CLAVE);
          }
        }
      } catch {
        // Sesión dañada: se pide ingresar otra vez.
      } finally {
        setCargando(false);
      }
    })();
  }, [salir]);

  const valor = useMemo<ContextoSesion>(
    () => ({
      cargando,
      sesion,
      ingresar: async (documento, contrasena) =>
        guardar(await api.post<Sesion>('/api/app/login', { documento, contrasena })),
      activar: async (documento, codigo, contrasena) =>
        guardar(await api.post<Sesion>('/api/app/activar', { documento, codigo, contrasena })),
      renovar: guardar,
      salir,
    }),
    [cargando, sesion, guardar, salir],
  );

  return <Contexto.Provider value={valor}>{children}</Contexto.Provider>;
}

export function useSesion(): ContextoSesion {
  const c = useContext(Contexto);
  if (!c) throw new Error('useSesion fuera de ProveedorSesion');
  return c;
}
