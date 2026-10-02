export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface EventListItem {
  id: number;
  name: string;
  slug: string;
  eventType: string;
  status: string;
  currency: string;
  startsAtUtc: string;
  endsAtUtc: string;
  salesStartUtc: string;
  salesEndUtc: string;
  venueName: string;
  venueCity: string;
  organizerName: string;
}

export interface EventPrice {
  seatCategoryId: number;
  categoryName: string;
  price: number;
  serviceFee: number;
}

export interface EventDetail {
  id: number;
  name: string;
  slug: string;
  eventType: string;
  status: string;
  currency: string;
  startsAtUtc: string;
  endsAtUtc: string;
  salesStartUtc: string;
  salesEndUtc: string;
  cancellationCutoffUtc: string;
  venueId: number;
  venueName: string;
  venueAddress: string;
  venueCity: string;
  venueCountry: string;
  organizerName: string;
  prices: EventPrice[];
}

export interface SectionAvailability {
  eventSectionId: number;
  sectionName: string;
  categoryName: string;
  isGeneralAdmission: boolean;
  capacity: number;
  sold: number;
  blocked: number;
}

export interface AvailableSeat {
  eventSeatId: number;
  seatId: number;
  sectionName: string;
  categoryName: string;
  rowLabel: string | null;
  seatNumber: string;
  price: number;
  serviceFee: number;
}

export interface Ticket {
  ticketId: number;
  ticketNumber: string;
  eventId: number;
  eventSectionId: number;
  eventSeatId: number | null;
  seatCategoryId: number;
  seatLabelSnapshot: string;
  pricePaid: number;
  serviceFeePaid: number;
  totalPaid: number;
  currency: string;
  purchasedAtUtc: string;
  status: string;
  cancelledAtUtc: string | null;
}

export interface TicketHistoryEntry {
  fromStatus: string | null;
  toStatus: string;
  changedAtUtc: string;
  reason: string | null;
}

export interface UserTicket {
  ticketId: number;
  ticketNumber: string;
  eventName: string;
  eventStartsAtUtc: string;
  venueName: string;
  seatLabel: string;
  categoryName: string;
  pricePaid: number;
  serviceFeePaid: number;
  currency: string;
  purchasedAtUtc: string;
  status: string;
  history: TicketHistoryEntry[];
}

export interface AuthResponse {
  token: string;
  expirationUtc: string;
  userId: number;
  email: string;
  fullName: string;
  role: string;
}

export interface RegisterRequest {
  email: string;
  fullName: string;
  password: string;
  phoneNumber?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface SimpleListItem {
  id: number;
  name: string;
}

export interface AdminEvent {
  id: number;
  name: string;
  venueId: number;
  organizerId: number;
  eventType: string;
  status: string;
  currency: string;
  startsAtUtc: string;
  endsAtUtc: string;
  salesStartUtc: string;
  salesEndUtc: string;
  cancellationCutoffUtc: string;
  posterUrl: string | null;
}

export interface CreateEventRequest {
  name: string;
  venueId: number;
  organizerId: number;
  eventType: string;
  currency: string;
  startsAtUtc: string;
  endsAtUtc: string;
  salesStartUtc: string;
  salesEndUtc: string;
  cancellationCutoffUtc: string;
}

export interface UpdateEventRequest {
  name: string;
  eventType: string;
  status: string;
  startsAtUtc: string;
  endsAtUtc: string;
  salesStartUtc: string;
  salesEndUtc: string;
  cancellationCutoffUtc: string;
}

export interface PriceInput {
  seatCategoryId: number;
  price: number;
  serviceFee: number;
}

export interface EventOccupancy {
  eventId: number;
  eventName: string;
  reservedCapacity: number;
  generalAdmissionCapacity: number;
  blockedSeats: number;
  soldTickets: number;
  cancelledTickets: number;
  sellableCapacity: number;
  grossCapacity: number;
  availableSeats: number;
  occupancyRate: number;
}

export interface ProblemDetails {
  title: string;
  status: number;
  detail: string;
  instance?: string;
}
