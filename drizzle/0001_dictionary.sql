CREATE TABLE "dictionaries" (
	"language_code" text NOT NULL,
	"dictionary_language_code" text NOT NULL,
	"source" text NOT NULL,
	"sha256" text NOT NULL,
	"imported_at" timestamp (6) with time zone NOT NULL,
	CONSTRAINT "dictionaries_language_code_dictionary_language_code_pk" PRIMARY KEY("language_code","dictionary_language_code")
);
--> statement-breakpoint
CREATE TABLE "dictionary_entries" (
	"language_code" text NOT NULL,
	"dictionary_language_code" text NOT NULL,
	"word" text NOT NULL,
	"meaning" text NOT NULL,
	CONSTRAINT "dictionary_entries_language_code_dictionary_language_code_word_pk" PRIMARY KEY("language_code","dictionary_language_code","word")
);
