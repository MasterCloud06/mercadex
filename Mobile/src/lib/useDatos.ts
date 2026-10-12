import { useFocusEffect } from 'expo-router';
import { useCallback, useRef, useState } from 'react';

/** Carga datos al entrar a la pantalla y permite recargar (deslizar hacia abajo). */
export function useDatos<T>(cargar: () => Promise<T>) {
  const [datos, setDatos] = useState<T | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [cargando, setCargando] = useState(true);
  const [recargando, setRecargando] = useState(false);
  const cargarRef = useRef(cargar);
  cargarRef.current = cargar;

  const ejecutar = useCallback(async (esRecarga: boolean) => {
    if (esRecarga) setRecargando(true);
    try {
      setDatos(await cargarRef.current());
      setError(null);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'No se pudo cargar.');
    } finally {
      setCargando(false);
      setRecargando(false);
    }
  }, []);

  useFocusEffect(
    useCallback(() => {
      void ejecutar(false);
    }, [ejecutar]),
  );

  return { datos, error, cargando, recargando, recargar: () => ejecutar(true) };
}
