// Identidad visual de Mercadex: azul institucional, superficie clara y
// un único acento amarillo maíz para lo que importa en cada pantalla.

export const colores = {
  azul: '#163A5B',
  azulMedio: '#1F4E79',
  tinta: '#12304D',
  texto: '#1E2A38',
  textoSuave: '#5A6B7D',
  textoTenue: '#8696A7',
  superficie: '#F3F6FA',
  blanco: '#FFFFFF',
  borde: '#E1E7EF',
  bordeCampo: '#C9D3DF',
  maiz: '#F2B705',
  verde: '#1D5C21',
  verdeFondo: '#EAF6EA',
  ambar: '#6E4F00',
  ambarFondo: '#FFF5D9',
  rojo: '#B42318',
  rojoFondo: '#FDE8E8',
} as const;

export const espacio = { xs: 4, s: 8, m: 12, l: 16, xl: 24, xxl: 32 } as const;

export const radio = { s: 8, m: 12, l: 16 } as const;

const formato = new Intl.NumberFormat('es-BO', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

/** "Bs 1.250,00" o "$us 20,00". Nunca mezcla monedas. */
export function dinero(monto: number, moneda: string): string {
  const simbolo = moneda === 'BOB' ? 'Bs' : moneda === 'USD' ? '$us' : moneda;
  return `${simbolo} ${formato.format(monto)}`;
}

const meses = ['ene', 'feb', 'mar', 'abr', 'may', 'jun', 'jul', 'ago', 'sep', 'oct', 'nov', 'dic'];

/** "12 oct 2026" (y la hora si se pide). */
export function fecha(valor: string, conHora = false): string {
  // "2026-09-28" es una fecha sin hora: se lee como día local, no UTC.
  const soloFecha = /^(\d{4})-(\d{2})-(\d{2})$/.exec(valor);
  const d = soloFecha
    ? new Date(Number(soloFecha[1]), Number(soloFecha[2]) - 1, Number(soloFecha[3]))
    : new Date(valor);
  if (Number.isNaN(d.getTime())) return valor;
  const base = `${d.getDate()} ${meses[d.getMonth()]} ${d.getFullYear()}`;
  if (!conHora) return base;
  const hh = String(d.getHours()).padStart(2, '0');
  const mm = String(d.getMinutes()).padStart(2, '0');
  return `${base}, ${hh}:${mm}`;
}
