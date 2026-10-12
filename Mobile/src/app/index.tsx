import { Redirect } from 'expo-router';
import { Cargando } from '@/componentes/ui';
import { useSesion } from '@/lib/sesion';

export default function Inicio() {
  const { cargando, sesion } = useSesion();
  if (cargando) return <Cargando texto="Abriendo Mercadex..." />;
  return <Redirect href={sesion ? '/(tabs)' : '/bienvenida'} />;
}
