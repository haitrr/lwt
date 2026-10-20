-- Compares term content like MySQL's utf8mb4_0900_ai_ci: ignoring case, accents and width.
CREATE COLLATION IF NOT EXISTS lwt_ci (provider = icu, locale = 'und-u-ks-level1', deterministic = false);
--> statement-breakpoint
CREATE TABLE "language_settings" (
	"id" serial PRIMARY KEY NOT NULL,
	"created_date" timestamp (6) with time zone NOT NULL,
	"last_modified_date" timestamp (6) with time zone NOT NULL,
	"dictionary_language_code" text NOT NULL,
	"language_code" text NOT NULL,
	"user_setting_id" integer
);
--> statement-breakpoint
CREATE TABLE "logs" (
	"id" serial PRIMARY KEY NOT NULL,
	"created_date" timestamp (6) with time zone NOT NULL,
	"last_modified_date" timestamp (6) with time zone NOT NULL,
	"event" text NOT NULL,
	"data" text NOT NULL,
	"user_name" text NOT NULL,
	"entity_id" integer,
	"entity_type" text
);
--> statement-breakpoint
CREATE TABLE "terms" (
	"id" serial PRIMARY KEY NOT NULL,
	"created_date" timestamp (6) with time zone NOT NULL,
	"last_modified_date" timestamp (6) with time zone NOT NULL,
	"content" varchar(95) COLLATE lwt_ci NOT NULL,
	"language_code" varchar(95) NOT NULL,
	"meaning" text NOT NULL,
	"user_id" integer NOT NULL,
	"learning_level" text NOT NULL
);
--> statement-breakpoint
CREATE TABLE "text_terms" (
	"id" serial PRIMARY KEY NOT NULL,
	"created_date" timestamp (6) with time zone NOT NULL,
	"last_modified_date" timestamp (6) with time zone NOT NULL,
	"text_id" integer NOT NULL,
	"term_id" integer,
	"content" text NOT NULL,
	"index" bigint NOT NULL
);
--> statement-breakpoint
CREATE TABLE "texts" (
	"id" serial PRIMARY KEY NOT NULL,
	"created_date" timestamp (6) with time zone NOT NULL,
	"last_modified_date" timestamp (6) with time zone NOT NULL,
	"user_id" integer NOT NULL,
	"bookmark" bigint,
	"title" text NOT NULL,
	"content" text NOT NULL,
	"language_code" text NOT NULL,
	"term_count" bigint NOT NULL,
	"processed_term_count" bigint NOT NULL,
	"last_read_at" timestamp (6) with time zone NOT NULL
);
--> statement-breakpoint
CREATE TABLE "user_settings" (
	"id" serial PRIMARY KEY NOT NULL,
	"created_date" timestamp (6) with time zone NOT NULL,
	"last_modified_date" timestamp (6) with time zone NOT NULL,
	"user_id" integer NOT NULL
);
--> statement-breakpoint
CREATE TABLE "users" (
	"id" serial PRIMARY KEY NOT NULL,
	"user_name" varchar(256) NOT NULL,
	"normalized_user_name" varchar(256) NOT NULL,
	"email" varchar(256),
	"password_hash" text NOT NULL
);
--> statement-breakpoint
ALTER TABLE "language_settings" ADD CONSTRAINT "language_settings_user_setting_id_user_settings_id_fk" FOREIGN KEY ("user_setting_id") REFERENCES "public"."user_settings"("id") ON DELETE no action ON UPDATE no action;--> statement-breakpoint
ALTER TABLE "terms" ADD CONSTRAINT "terms_user_id_users_id_fk" FOREIGN KEY ("user_id") REFERENCES "public"."users"("id") ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
ALTER TABLE "text_terms" ADD CONSTRAINT "text_terms_text_id_texts_id_fk" FOREIGN KEY ("text_id") REFERENCES "public"."texts"("id") ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
ALTER TABLE "text_terms" ADD CONSTRAINT "text_terms_term_id_terms_id_fk" FOREIGN KEY ("term_id") REFERENCES "public"."terms"("id") ON DELETE no action ON UPDATE no action;--> statement-breakpoint
ALTER TABLE "texts" ADD CONSTRAINT "texts_user_id_users_id_fk" FOREIGN KEY ("user_id") REFERENCES "public"."users"("id") ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
ALTER TABLE "user_settings" ADD CONSTRAINT "user_settings_user_id_users_id_fk" FOREIGN KEY ("user_id") REFERENCES "public"."users"("id") ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
CREATE INDEX "language_settings_user_setting_id_idx" ON "language_settings" USING btree ("user_setting_id");--> statement-breakpoint
CREATE UNIQUE INDEX "terms_content_language_code_user_id_key" ON "terms" USING btree ("content","language_code","user_id");--> statement-breakpoint
CREATE INDEX "terms_user_id_idx" ON "terms" USING btree ("user_id");--> statement-breakpoint
CREATE INDEX "text_terms_text_id_index_idx" ON "text_terms" USING btree ("text_id","index");--> statement-breakpoint
CREATE INDEX "text_terms_text_id_term_id_idx" ON "text_terms" USING btree ("text_id","term_id");--> statement-breakpoint
CREATE INDEX "text_terms_term_id_idx" ON "text_terms" USING btree ("term_id");--> statement-breakpoint
CREATE INDEX "texts_user_id_idx" ON "texts" USING btree ("user_id");--> statement-breakpoint
CREATE INDEX "texts_processed_term_count_idx" ON "texts" USING btree ("processed_term_count");--> statement-breakpoint
CREATE UNIQUE INDEX "user_settings_user_id_key" ON "user_settings" USING btree ("user_id");--> statement-breakpoint
CREATE UNIQUE INDEX "users_normalized_user_name_key" ON "users" USING btree ("normalized_user_name");