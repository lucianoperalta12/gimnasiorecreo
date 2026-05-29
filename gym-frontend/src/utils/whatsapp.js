/**
 * Normaliza un teléfono para wa.me (solo dígitos, con código de país).
 * Prioriza formato argentino (54 + móvil).
 * @returns {string|null}
 */
export function normalizePhoneForWhatsApp(telefono) {
  if (!telefono || typeof telefono !== 'string') return null;

  let digits = telefono.replace(/\D/g, '');
  if (!digits) return null;

  while (digits.startsWith('0')) {
    digits = digits.slice(1);
  }

  if (digits.startsWith('00')) {
    digits = digits.slice(2);
  }

  if (digits.startsWith('54')) {
    if (digits.length < 12 || digits.length > 13) return null;
    return isReasonablePhone(digits) ? digits : null;
  }

  if (digits.length >= 12 && digits.length <= 15) {
    return isReasonablePhone(digits) ? digits : null;
  }

  if (digits.length === 10) {
    digits = `549${digits}`;
  } else if (digits.length === 11 && digits.startsWith('9')) {
    digits = `54${digits}`;
  } else {
    return null;
  }

  if (digits.length < 12 || digits.length > 15) return null;
  return isReasonablePhone(digits) ? digits : null;
}

export function isValidPhoneForWhatsApp(telefono) {
  return normalizePhoneForWhatsApp(telefono) !== null;
}

function isReasonablePhone(digits) {
  if (digits.length < 10 || digits.length > 15) return false;
  if (/^(\d)\1{7,}$/.test(digits)) return false;
  return true;
}

export function getFirstName(fullName) {
  const parts = (fullName || '').trim().split(/\s+/);
  return parts[0] || '';
}

export function buildMembresiaVencidaWhatsAppMessage(alumnoNombreCompleto, gymNombre) {
  const nombre = getFirstName(alumnoNombreCompleto) || 'alumno';
  const gym = (gymNombre || 'Gimnasio').trim();

  return `Hola ${nombre}, ¿cómo estás?

Te recordamos que tu cuota del gimnasio se encuentra vencida. Te pedimos que regularices el pago para mantener tu acceso activo y seguir disfrutando de las actividades del gimnasio.

Muchas gracias.

Saludos,
${gym}`;
}

export function buildWhatsAppUrl(telefono, message) {
  const phone = normalizePhoneForWhatsApp(telefono);
  if (!phone) return null;
  return `https://wa.me/${phone}?text=${encodeURIComponent(message)}`;
}
