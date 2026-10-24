ALTER TABLE "terms" ADD COLUMN "phrase" boolean DEFAULT false NOT NULL;--> statement-breakpoint
ALTER TABLE "terms" ADD COLUMN "auto_join" boolean DEFAULT false NOT NULL;