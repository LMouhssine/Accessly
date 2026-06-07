import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  AiTagsResponse,
  AiTextResponse,
  AuditLogEntry,
  Booking,
  BookingResult,
  CheckIn,
  DashboardSummary,
  CheckInResponse,
  CheckInSummary,
  EventDetail,
  EventStatus,
  EventSummary,
  Feedback,
  FeedbackList,
  NotificationItem,
  Organization,
  PagedResult,
  Ticket,
} from './models';

export interface EventInput {
  title: string;
  description: string;
  category?: string | null;
  startAt: string;
  endAt: string;
  venueName: string;
  venueAddress?: string | null;
  capacity: number;
  priceAmount: number;
  currency: string;
  coverImageUrl?: string | null;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;

  // --- Dashboard ---
  getDashboardSummary(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(`${this.base}/api/dashboard/summary`);
  }

  // --- Events ---
  getEvents(options: {
    page?: number;
    pageSize?: number;
    search?: string;
    category?: string;
    status?: EventStatus;
    publishedOnly?: boolean;
    organizationId?: string;
  } = {}): Observable<PagedResult<EventSummary>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(options)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return this.http.get<PagedResult<EventSummary>>(`${this.base}/api/events`, { params });
  }

  getEvent(id: string): Observable<EventDetail> {
    return this.http.get<EventDetail>(`${this.base}/api/events/${id}`);
  }

  createEvent(input: EventInput): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.base}/api/events`, input);
  }

  updateEvent(id: string, input: EventInput): Observable<void> {
    return this.http.put<void>(`${this.base}/api/events/${id}`, input);
  }

  publishEvent(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/api/events/${id}/publish`, {});
  }

  unpublishEvent(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/api/events/${id}/unpublish`, {});
  }

  cancelEvent(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/api/events/${id}/cancel`, {});
  }

  // --- Bookings ---
  createBooking(eventId: string): Observable<BookingResult> {
    return this.http.post<BookingResult>(`${this.base}/api/events/${eventId}/bookings`, {});
  }

  getBookings(options: { eventId?: string; page?: number; pageSize?: number } = {}): Observable<PagedResult<Booking>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(options)) {
      if (value !== undefined && value !== null) {
        params = params.set(key, String(value));
      }
    }
    return this.http.get<PagedResult<Booking>>(`${this.base}/api/bookings`, { params });
  }

  cancelBooking(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/api/bookings/${id}/cancel`, {});
  }

  // --- Tickets ---
  getTicket(id: string): Observable<Ticket> {
    return this.http.get<Ticket>(`${this.base}/api/tickets/${id}`);
  }

  getTicketQr(id: string): Observable<Blob> {
    return this.http.get(`${this.base}/api/tickets/${id}/qr`, { responseType: 'blob' });
  }

  // --- Check-ins ---
  recordCheckIn(eventId: string, code: string): Observable<CheckInResponse> {
    return this.http.post<CheckInResponse>(`${this.base}/api/check-ins`, { code, eventId });
  }

  getCheckInSummary(eventId: string): Observable<CheckInSummary> {
    return this.http.get<CheckInSummary>(`${this.base}/api/events/${eventId}/check-ins/summary`);
  }

  getCheckIns(eventId: string): Observable<CheckIn[]> {
    return this.http.get<CheckIn[]>(`${this.base}/api/events/${eventId}/check-ins`);
  }

  // --- Notifications ---
  getNotifications(): Observable<NotificationItem[]> {
    return this.http.get<NotificationItem[]>(`${this.base}/api/notifications`);
  }

  sendTestNotification(): Observable<NotificationItem> {
    return this.http.post<NotificationItem>(`${this.base}/api/notifications/test`, {});
  }

  // --- Feedback ---
  submitFeedback(eventId: string, rating: number, comment?: string): Observable<Feedback> {
    return this.http.post<Feedback>(`${this.base}/api/events/${eventId}/feedbacks`, { rating, comment });
  }

  getFeedbacks(eventId: string): Observable<FeedbackList> {
    return this.http.get<FeedbackList>(`${this.base}/api/events/${eventId}/feedbacks`);
  }

  summarizeFeedback(eventId: string): Observable<AiTextResponse> {
    return this.http.post<AiTextResponse>(`${this.base}/api/events/${eventId}/feedbacks/ai-summary`, {});
  }

  // --- AI assistant ---
  aiEventDescription(payload: Record<string, unknown>): Observable<AiTextResponse> {
    return this.http.post<AiTextResponse>(`${this.base}/api/ai/event-description`, payload);
  }

  aiEventTags(payload: Record<string, unknown>): Observable<AiTagsResponse> {
    return this.http.post<AiTagsResponse>(`${this.base}/api/ai/event-tags`, payload);
  }

  // --- Organizations ---
  getOrganizations(): Observable<Organization[]> {
    return this.http.get<Organization[]>(`${this.base}/api/organizations`);
  }

  // --- Audit logs ---
  getAuditLogs(options: {
    page?: number;
    pageSize?: number;
    action?: string;
    entityType?: string;
    actorId?: string;
  } = {}): Observable<PagedResult<AuditLogEntry>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(options)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return this.http.get<PagedResult<AuditLogEntry>>(`${this.base}/api/audit-logs`, { params });
  }
}
