export type UserRole = 'Admin' | 'Organizer' | 'Staff' | 'Attendee';
export type EventStatus = 'Draft' | 'Published' | 'Cancelled' | 'Completed';
export type BookingStatus = 'Pending' | 'Confirmed' | 'Cancelled' | 'Expired';
export type TicketStatus = 'Active' | 'Used' | 'Cancelled';
export type CheckInResult = 'Accepted' | 'AlreadyCheckedIn' | 'InvalidTicket' | 'TicketCancelled' | 'EventMismatch';

export interface AuthenticatedUser {
  id: string;
  email: string;
  displayName: string;
  role: UserRole;
  organizationId?: string | null;
}

export interface LoginResponse {
  accessToken: string;
  expiresAt: string;
  user: AuthenticatedUser;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface Speaker {
  id: string;
  name: string;
  title?: string | null;
  bio?: string | null;
}

export interface EventSummary {
  id: string;
  title: string;
  slug: string;
  category?: string | null;
  startAt: string;
  endAt: string;
  venueName: string;
  capacity: number;
  bookedCount: number;
  priceAmount: number;
  currency: string;
  status: EventStatus;
  coverImageUrl?: string | null;
  organizationId: string;
}

export interface EventDetail extends EventSummary {
  description: string;
  venueAddress?: string | null;
  speakers: Speaker[];
}

export interface BookingResult {
  bookingId: string;
  ticketId: string;
  ticketCode: string;
  status: BookingStatus;
}

export interface Booking {
  id: string;
  eventId: string;
  eventTitle: string;
  eventStartAt: string;
  attendeeUserId: string;
  attendeeName: string;
  status: BookingStatus;
  createdAt: string;
  ticketId?: string | null;
  ticketCode?: string | null;
  ticketStatus?: TicketStatus | null;
}

export interface Ticket {
  id: string;
  bookingId: string;
  eventId: string;
  eventTitle: string;
  code: string;
  status: TicketStatus;
  issuedAt: string;
  usedAt?: string | null;
  attendeeName: string;
}

export interface CheckIn {
  id: string;
  ticketId: string;
  ticketCode: string;
  attendeeName: string;
  checkedInAt: string;
  result: CheckInResult;
}

export interface CheckInSummary {
  eventId: string;
  registered: number;
  checkedIn: number;
  capacity: number;
  fillRate: number;
  recent: CheckIn[];
}

export interface CheckInResponse {
  result: CheckInResult;
  accepted: boolean;
  message: string;
  checkIn?: CheckIn | null;
}

export interface NotificationItem {
  id: string;
  type: string;
  subject: string;
  body: string;
  status: string;
  createdAt: string;
  sentAt?: string | null;
  toAddress?: string | null;
}

export interface Feedback {
  id: string;
  eventId: string;
  rating: number;
  comment?: string | null;
  createdAt: string;
}

export interface FeedbackList {
  eventId: string;
  count: number;
  averageRating: number;
  items: Feedback[];
}

export interface AiTextResponse {
  provider: string;
  text: string;
}

export interface AiTagsResponse {
  provider: string;
  tags: string[];
}

export interface Organization {
  id: string;
  name: string;
  slug: string;
  createdAt: string;
}
