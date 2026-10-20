ALTER TABLE "texts" ADD COLUMN "processing_error" text;--> statement-breakpoint
ALTER TABLE "texts" ADD COLUMN "failed_at" timestamp (6) with time zone;--> statement-breakpoint
CREATE INDEX "logs_user_name_event_id_idx" ON "logs" USING btree ("user_name","event","id" DESC NULLS LAST);